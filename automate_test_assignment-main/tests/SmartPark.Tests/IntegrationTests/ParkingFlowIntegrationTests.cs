using Moq;
using SmartPark.Core.Interfaces;
using SmartPark.Core.Models;
using SmartPark.Core.Services;

namespace SmartPark.Tests.IntegrationTests;

public class ParkingFlowIntegrationTests
{
    // ────────────────────────────────────────────────────────────
    //  INTEGRATION TEST SETUP
    //  Uses REAL components for business logic, and TEST DOUBLES
    //  only for external boundaries:
    //
    //  Real objects:
    //    ParkingFeeCalculator       — real (pure logic, no side effects)
    //    InMemoryParkingRepository  — fake (working in-memory implementation)
    //
    //  Test doubles (via Moq, used as stubs here):
    //    IPaymentGateway            — stub (always returns success)
    //    INotificationService       — stub (does nothing)
    //    IDateTimeProvider          — stub (returns controlled time)
    //    IMembershipService         — stub (returns Guest for all)
    // ────────────────────────────────────────────────────────────

    private readonly ParkingFeeCalculator _feeCalculator = new();
    private readonly InMemoryParkingRepository _repository = new();  // fake
    private readonly Mock<IPaymentGateway> _paymentStub = new();
    private readonly Mock<INotificationService> _notificationStub = new();
    private readonly ParkingSessionManager _manager;

    // Fake clock — set this in each test to control time
    private DateTime _currentTime = new(2026, 3, 16, 10, 0, 0); // Monday 10 AM

    public ParkingFlowIntegrationTests()
    {
        var dateTimeStub = new Mock<IDateTimeProvider>();
        dateTimeStub.Setup(d => d.Now).Returns(() => _currentTime);

        var membershipStub = new Mock<IMembershipService>();
        membershipStub.Setup(m => m.GetMembershipTier(It.IsAny<string>())).Returns(MembershipTier.Guest);

        _paymentStub.Setup(p => p.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(true);

        _manager = new ParkingSessionManager(
            _feeCalculator,
            _paymentStub.Object,
            _notificationStub.Object,
            membershipStub.Object,
            _repository,          // real fake, not a Moq object
            dateTimeStub.Object);
    }

    // ────────────────────────────────────────────────────────────
    //  EXAMPLE TEST — shows how to advance time between operations.
    //  Delete or keep this; it does not count toward your grade.
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task FullFlow_CheckInAndCheckOut_CalculatesCorrectFee()
    {
        // Arrange — check in at 10:00 AM
        _currentTime = new DateTime(2026, 3, 16, 10, 0, 0); // Monday
        var ticket = await _manager.CheckInAsync("TEST-001", VehicleType.Car);

        // Act — check out at 12:30 PM (2.5 hours later → 2 billable hours after grace)
        _currentTime = new DateTime(2026, 3, 16, 12, 30, 0);
        var result = await _manager.CheckOutAsync(ticket.TicketId, "012-345-678");

        // Assert — Car: 2 hours × 1,000 = 2,000 KHR
        Assert.Equal(2_000m, result.TotalFee);
    }

    #region Full Parking Flow
    [Fact]
    public async Task FullFlow_ComplexScenario_ChecksInAndChecksOut_WithAllModifiers()
    {
        // Saturday 8 PM (Weekend Surcharge, later Overnight Fee)
        _currentTime = new DateTime(2026, 3, 21, 20, 0, 0); 
        var ticket = await _manager.CheckInAsync("TEST-002", VehicleType.SUV);
        
        var activeTicket = await _repository.GetActiveTicketByPlateAsync("TEST-002");
        Assert.NotNull(activeTicket);
        
        // Check out at 1 AM the next day (5 hours total -> 4.5 hrs post-grace -> 5 billable hours)
        _currentTime = new DateTime(2026, 3, 22, 1, 0, 0);
        var result = await _manager.CheckOutAsync(ticket.TicketId, "012-345-678");
        
        // Base Fee: SUV 1500/hr * 5 hrs = 7500.
        // Weekend Surcharge: 20% of 7500 = 1500.
        // Overnight Fee: 2000.
        // Total = 7500 + 2000 + 1500 = 11000 KHR.
        Assert.Equal(11000m, result.TotalFee);
        
        var inactiveTicket = await _repository.GetActiveTicketByPlateAsync("TEST-002");
        Assert.Null(inactiveTicket);
        
        var completedTicket = await _repository.GetTicketByIdAsync(ticket.TicketId);
        Assert.NotNull(completedTicket);
        Assert.NotNull(completedTicket.CheckOutTime);
    }
    #endregion

    #region Multiple Vehicles
    [Fact]
    public async Task MultipleVehicles_CheckIn3_CheckOut1_TwoRemainActive()
    {
        _currentTime = new DateTime(2026, 3, 16, 10, 0, 0);
        var t1 = await _manager.CheckInAsync("CAR-001", VehicleType.Car);
        var t2 = await _manager.CheckInAsync("CAR-002", VehicleType.Motorcycle);
        var t3 = await _manager.CheckInAsync("CAR-003", VehicleType.SUV);

        _currentTime = new DateTime(2026, 3, 16, 12, 0, 0);
        await _manager.CheckOutAsync(t1.TicketId, "012-345-678");

        var a1 = await _repository.GetActiveTicketByPlateAsync("CAR-001");
        var a2 = await _repository.GetActiveTicketByPlateAsync("CAR-002");
        var a3 = await _repository.GetActiveTicketByPlateAsync("CAR-003");

        Assert.Null(a1);
        Assert.NotNull(a2);
        Assert.NotNull(a3);
    }
    #endregion

    #region Error Recovery
    [Fact]
    public async Task ErrorRecovery_FailedPayment_TicketRemainsActive()
    {
        _currentTime = new DateTime(2026, 3, 16, 10, 0, 0);
        var ticket = await _manager.CheckInAsync("ERR-001", VehicleType.Car);

        _paymentStub.Setup(p => p.ProcessPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>())).ReturnsAsync(false);
        _currentTime = new DateTime(2026, 3, 16, 12, 0, 0);

        await Assert.ThrowsAsync<Exception>(() => _manager.CheckOutAsync(ticket.TicketId, "012-345-678"));

        var active = await _repository.GetActiveTicketByPlateAsync("ERR-001");
        Assert.NotNull(active);
        Assert.Null(active.CheckOutTime);
    }

    [Fact]
    public async Task ErrorRecovery_DuplicateCheckIn_ThrowsAndOriginalRemains()
    {
        _currentTime = new DateTime(2026, 3, 16, 10, 0, 0);
        await _manager.CheckInAsync("DUP-001", VehicleType.Car);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _manager.CheckInAsync("DUP-001", VehicleType.Motorcycle));

        var active = await _repository.GetActiveTicketByPlateAsync("DUP-001");
        Assert.NotNull(active);
        Assert.Equal(VehicleType.Car, active.Vehicle.Type);
    }
    #endregion

    #region Edge-to-Edge Scenarios
    [Fact]
    public async Task EdgeToEdge_GracePeriod_CheckOutWithin15Min_FreeParking()
    {
        _currentTime = new DateTime(2026, 3, 16, 10, 0, 0);
        var ticket = await _manager.CheckInAsync("GRACE-001", VehicleType.SUV);

        _currentTime = new DateTime(2026, 3, 16, 10, 15, 0);
        var result = await _manager.CheckOutAsync(ticket.TicketId, "012-345-678");

        Assert.Equal(0m, result.TotalFee);
    }
    #endregion
}
