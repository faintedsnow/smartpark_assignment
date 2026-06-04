using SmartPark.Core.Models;
using SmartPark.Core.Services;
using FsCheck;
using FsCheck.Xunit;

namespace SmartPark.Tests;

public class ParkingFeeCalculatorTests
{
    private readonly ParkingFeeCalculator _calculator = new();

    // ────────────────────────────────────────────────────────────
    //  EXAMPLE TEST — shows the naming convention and AAA pattern.
    //  Delete or keep this; it does not count toward your grade.
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateFee_ZeroDuration_ReturnsFree()
    {
        // Arrange
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);  // Monday
        var checkOut = checkIn; // same time = 0 duration

        // Act
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut);

        // Assert
        Assert.Equal(0m, result.TotalFee);
    }

    #region Basic Fee Calculation
    // Test basic hourly rates for each vehicle type
    // Consider using [Theory] with [InlineData] for multiple scenarios

    [Fact]
    public void CalculateFee_Motorcycle_2Hours_Returns1000()
    {
        // Arrange
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0); // Monday, not a holiday
        var checkOut = checkIn.AddHours(2); // exactly 2 hours later

        // Act
        var result = _calculator.CalculateFee(
            VehicleType.Motorcycle, 
            MembershipTier.Guest, 
            checkIn, 
            checkOut);

        // Assert
        Assert.Equal(1000m, result.TotalFee);
    }

    [Fact]
    public void CalculateFee_Car_3Hours_Returns3000()
    {
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
        var checkOut = checkIn.AddHours(3);
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut);
        Assert.Equal(3000m, result.TotalFee);
    }

    [Fact]
    public void CalculateFee_SUV_1Hour_Returns1500()
    {
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
        var checkOut = checkIn.AddHours(1);
        var result = _calculator.CalculateFee(VehicleType.SUV, MembershipTier.Guest, checkIn, checkOut);
        Assert.Equal(1500m, result.TotalFee);
    }
    #endregion

    #region Grace Period
    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(29)]
    [InlineData(30)]
    public void CalculateFee_GracePeriod_30MinutesOrLess_ReturnsFree(int minutes)
    {
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
        var checkOut = checkIn.AddMinutes(minutes);
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut);
        Assert.Equal(0m, result.TotalFee);
    }
    #endregion

    #region Duration Rounding
    [Theory]
    [InlineData(31, 1)]
    [InlineData(60, 1)]
    [InlineData(61, 2)]
    [InlineData(90, 1)]
    [InlineData(91, 2)]
    [InlineData(150, 2)]
    [InlineData(151, 3)]
    public void CalculateFee_DurationRounding_AlwaysRoundsUp(int totalMinutes, int expectedBillableHours)
    {
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
        var checkOut = checkIn.AddMinutes(totalMinutes);
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut);
        Assert.Equal(expectedBillableHours * 1000m, result.TotalFee);
    }
    #endregion

    #region Daily Cap
    [Fact]
    public void CalculateFee_DailyCap_Motorcycle_10Hours_CappedAt4000()
    {
        var checkIn = new DateTime(2026, 3, 16, 8, 0, 0);
        var checkOut = checkIn.AddHours(10);
        var result = _calculator.CalculateFee(VehicleType.Motorcycle, MembershipTier.Guest, checkIn, checkOut);
        Assert.Equal(4000m, result.TotalFee);
    }
    #endregion

    #region Overnight Fee
    [Fact]
    public void CalculateFee_Overnight_Car_4Hours_Returns6000()
    {
        var checkIn = new DateTime(2026, 3, 16, 20, 0, 0); // 8 PM
        var checkOut = checkIn.AddHours(4); // 12 AM -> Base 4000 + 2000 = 6000
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut);
        Assert.Equal(6000m, result.TotalFee);
    }
    
    [Fact]
    public void CalculateFee_NoOvernight_Car_9Hours_Returns8000()
    {
        var checkIn = new DateTime(2026, 3, 16, 8, 0, 0); // 8 AM
        var checkOut = checkIn.AddHours(9); // 5 PM
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut);
        Assert.Equal(8000m, result.TotalFee);
    }
    #endregion

    #region Weekend Surcharge
    [Fact]
    public void CalculateFee_WeekendSurcharge_Saturday_Car_2Hours_Returns2400()
    {
        // 2026-03-21 is a Saturday
        var checkIn = new DateTime(2026, 3, 21, 10, 0, 0);
        var checkOut = checkIn.AddHours(2); // Base: 2000, Surcharge 20%: 400 => 2400
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut);
        Assert.Equal(2400m, result.TotalFee);
    }
    #endregion

    #region Holiday Surcharge
    [Fact]
    public void CalculateFee_HolidaySurcharge_TakesPriorityOverWeekend()
    {
        // 2026-03-21 is a Saturday. Also a holiday.
        var checkIn = new DateTime(2026, 3, 21, 10, 0, 0);
        var checkOut = checkIn.AddHours(2); // Base: 2000, Holiday 50%: 1000 => 3000
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut, isHoliday: true);
        Assert.Equal(3000m, result.TotalFee);
    }
    #endregion

    #region Membership Discounts
    [Fact]
    public void CalculateFee_MembershipDiscount_Gold_25PercentOff()
    {
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0); // Monday
        var checkOut = checkIn.AddHours(2); // Car base: 2000
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Gold, checkIn, checkOut);
        Assert.Equal(1500m, result.TotalFee);
    }
    #endregion

    #region Lost Ticket
    [Fact]
    public void CalculateFee_LostTicket_Car_2Hours_Returns22000()
    {
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0); // Monday
        var checkOut = checkIn.AddHours(2); // Car base: 2000
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut, isLostTicket: true);
        Assert.Equal(22000m, result.TotalFee);
    }
    #endregion

    #region Edge Cases
    // Test invalid inputs and boundary conditions
    #endregion

    #region Property-Based Tests
    // Write at least 5 FsCheck properties that must hold for ALL valid inputs
    // You may need custom Arbitrary<T> for generating valid DateTime pairs
    #endregion
}
