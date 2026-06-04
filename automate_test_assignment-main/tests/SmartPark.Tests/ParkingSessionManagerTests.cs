using Moq;
using SmartPark.Core.Interfaces;
using SmartPark.Core.Models;
using SmartPark.Core.Services;

namespace SmartPark.Tests;

public class ParkingSessionManagerTests
{
    // ────────────────────────────────────────────────────────────
    //  SHARED SETUP — create test doubles and the system-under-test.
    //  Moq's Mock<T> creates test doubles that can act as:
    //    - Stubs: .Setup().Returns() — provide canned answers
    //    - Mocks: .Verify()         — assert interactions happened
    //  You can use a constructor, or duplicate this in each test.
    // ────────────────────────────────────────────────────────────

    private readonly Mock<IPaymentGateway> _paymentStub = new();
    private readonly Mock<INotificationService> _notificationStub = new();
    private readonly Mock<IMembershipService> _membershipStub = new();
    private readonly Mock<IParkingRepository> _repoStub = new();
    private readonly Mock<IDateTimeProvider> _dateTimeStub = new();
    private readonly ParkingFeeCalculator _feeCalculator = new();
    private readonly ParkingSessionManager _manager;

    public ParkingSessionManagerTests()
    {
        _manager = new ParkingSessionManager(
            _feeCalculator,
            _paymentStub.Object,
            _notificationStub.Object,
            _membershipStub.Object,
            _repoStub.Object,
            _dateTimeStub.Object);
    }

    // ────────────────────────────────────────────────────────────
    //  EXAMPLE TEST — shows stub setup + mock verification pattern.
    //  .Setup().Returns() = STUB behavior (canned answer)
    //  .Verify()          = MOCK behavior (interaction assertion)
    //  Delete or keep this; it does not count toward your grade.
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckInAsync_NewVehicle_LookUpMembership()
    {
        // Arrange — configure stubs (canned return values)
        _membershipStub.Setup(m => m.GetMembershipTier("PP-9999")).Returns(MembershipTier.Guest);
        _repoStub.Setup(r => r.GetActiveTicketByPlateAsync("PP-9999")).ReturnsAsync((ParkingTicket?)null);
        _dateTimeStub.Setup(d => d.Now).Returns(new DateTime(2026, 3, 16, 10, 0, 0));

        // Act
        var ticket = await _manager.CheckInAsync("PP-9999", VehicleType.Car);

        // Assert — verify as mock (was this interaction called?)
        _membershipStub.Verify(m => m.GetMembershipTier("PP-9999"), Times.Once);
        Assert.Equal("PP-9999", ticket.Vehicle.LicensePlate);
    }

    #region CheckIn — Happy Path
    [Fact]
    public async Task CheckInAsync_Successful_SavesTicketAndLooksUpMembership()
    {
        _membershipStub.Setup(m => m.GetMembershipTier("PP-9999")).Returns(MembershipTier.Guest);
        _repoStub.Setup(r => r.GetActiveTicketByPlateAsync("PP-9999")).ReturnsAsync((ParkingTicket?)null);
        _dateTimeStub.Setup(d => d.Now).Returns(new DateTime(2026, 3, 16, 10, 0, 0));

        var ticket = await _manager.CheckInAsync("PP-9999", VehicleType.Car);

        _membershipStub.Verify(m => m.GetMembershipTier("PP-9999"), Times.Once);
        _repoStub.Verify(r => r.SaveTicketAsync(It.IsAny<ParkingTicket>()), Times.Once);
        Assert.NotNull(ticket);
    }
    #endregion

    #region CheckIn — Validation
    [Fact]
    public async Task CheckInAsync_Duplicate_ThrowsInvalidOperationException_DoesNotSave()
    {
        var existingTicket = new ParkingTicket();
        _repoStub.Setup(r => r.GetActiveTicketByPlateAsync("PP-9999")).ReturnsAsync(existingTicket);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _manager.CheckInAsync("PP-9999", VehicleType.Car));

        _repoStub.Verify(r => r.SaveTicketAsync(It.IsAny<ParkingTicket>()), Times.Never);
    }
    #endregion

    #region CheckOut — Happy Path
    [Fact]
    public async Task CheckOutAsync_HappyPath_UpdatesTicketAndSendsReceipt()
    {
        var ticket = new ParkingTicket 
        { 
            TicketId = "T-123", 
            Vehicle = new Vehicle { LicensePlate = "PP-1111", Type = VehicleType.Car }, 
            CheckInTime = new DateTime(2026, 3, 16, 10, 0, 0)
        };
        _repoStub.Setup(r => r.GetTicketByIdAsync("T-123")).ReturnsAsync(ticket);
        _dateTimeStub.Setup(d => d.Now).Returns(new DateTime(2026, 3, 16, 12, 0, 0));
        _paymentStub.Setup(p => p.ProcessPaymentAsync("T-123", It.IsAny<decimal>())).ReturnsAsync(true);

        var result = await _manager.CheckOutAsync("T-123", "012-345-678");

        _repoStub.Verify(r => r.UpdateTicketAsync(ticket), Times.Once);
        _notificationStub.Verify(n => n.SendReceiptAsync("012-345-678", It.IsAny<string>()), Times.Once);
        Assert.NotNull(result);
    }
    #endregion

    #region CheckOut — Payment Failure
    [Fact]
    public async Task CheckOutAsync_PaymentFailure_ThrowsException_DoesNotUpdateOrSendReceipt()
    {
        var ticket = new ParkingTicket 
        { 
            TicketId = "T-123", 
            Vehicle = new Vehicle { LicensePlate = "PP-1111", Type = VehicleType.Car }, 
            CheckInTime = new DateTime(2026, 3, 16, 10, 0, 0)
        };
        _repoStub.Setup(r => r.GetTicketByIdAsync("T-123")).ReturnsAsync(ticket);
        _dateTimeStub.Setup(d => d.Now).Returns(new DateTime(2026, 3, 16, 12, 0, 0));
        _paymentStub.Setup(p => p.ProcessPaymentAsync("T-123", It.IsAny<decimal>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<Exception>(() => _manager.CheckOutAsync("T-123", "012-345-678"));

        _repoStub.Verify(r => r.UpdateTicketAsync(It.IsAny<ParkingTicket>()), Times.Never);
        _notificationStub.Verify(n => n.SendReceiptAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    #endregion

    #region CheckOut — Notification Failure
    [Fact]
    public async Task CheckOutAsync_NotificationFailure_StillSucceeds_GracefulDegradation()
    {
        var ticket = new ParkingTicket 
        { 
            TicketId = "T-123", 
            Vehicle = new Vehicle { LicensePlate = "PP-1111", Type = VehicleType.Car }, 
            CheckInTime = new DateTime(2026, 3, 16, 10, 0, 0)
        };
        _repoStub.Setup(r => r.GetTicketByIdAsync("T-123")).ReturnsAsync(ticket);
        _dateTimeStub.Setup(d => d.Now).Returns(new DateTime(2026, 3, 16, 12, 0, 0));
        _paymentStub.Setup(p => p.ProcessPaymentAsync("T-123", It.IsAny<decimal>())).ReturnsAsync(true);
        _notificationStub.Setup(n => n.SendReceiptAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("SMS gateway down"));

        var result = await _manager.CheckOutAsync("T-123", "012-345-678");

        _repoStub.Verify(r => r.UpdateTicketAsync(ticket), Times.Once);
        Assert.NotNull(result);
        Assert.Equal(2000m, result.TotalFee);
    }
    #endregion

    #region CheckOut — Validation
    // Test check-out error scenarios for missing or invalid tickets
    #endregion

    #region Verify Interaction Order
    // Verify that dependencies are called in the correct sequence
    #endregion
}
