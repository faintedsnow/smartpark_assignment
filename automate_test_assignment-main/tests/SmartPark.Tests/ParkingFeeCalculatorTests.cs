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
    [InlineData(61, 1)]
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
    [Fact]
    public void CalculateFee_CheckOutBeforeCheckIn_ThrowsArgumentException()
    {
        var checkIn = new DateTime(2026, 3, 16, 12, 0, 0);
        var checkOut = new DateTime(2026, 3, 16, 10, 0, 0);
        Assert.Throws<ArgumentException>(() => _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut));
    }

    [Fact]
    public void CalculateFee_ZeroDuration_ReturnsFree_Duplicate()
    {
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
        var result = _calculator.CalculateFee(VehicleType.SUV, MembershipTier.Platinum, checkIn, checkIn);
        Assert.Equal(0m, result.TotalFee);
    }

    [Fact]
    public void CalculateFee_ExactGraceBoundary_31Minutes_Returns1Hour()
    {
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
        var checkOut = checkIn.AddMinutes(31);
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut);
        Assert.Equal(1000m, result.TotalFee);
    }
    #endregion

    #region Property-Based Tests
    [Property]
    public void Fee_IsNeverNegative(VehicleType vehicleType, MembershipTier membership, DateTime checkIn, int durationMinutes, bool isLostTicket, bool isHoliday)
    {
        durationMinutes = Math.Abs(durationMinutes % 500000);
        var checkOut = checkIn.AddMinutes(durationMinutes);
        var result = _calculator.CalculateFee(vehicleType, membership, checkIn, checkOut, isLostTicket, isHoliday);
        Assert.True(result.TotalFee >= 0);
    }

    [Property]
    public void GracePeriod_IsAlwaysZero(VehicleType vehicleType, MembershipTier membership, DateTime checkIn, int durationMinutes, bool isLostTicket, bool isHoliday)
    {
        durationMinutes = Math.Abs(durationMinutes % 31);
        var checkOut = checkIn.AddMinutes(durationMinutes);
        var result = _calculator.CalculateFee(vehicleType, membership, checkIn, checkOut, isLostTicket, isHoliday);
        Assert.Equal(0m, result.TotalFee);
    }

    [Property]
    public void CheckOutBeforeCheckIn_ThrowsArgumentException(VehicleType vehicleType, MembershipTier membership, DateTime checkIn, int negativeDuration, bool isLostTicket, bool isHoliday)
    {
        negativeDuration = -Math.Abs(negativeDuration % 500000) - 1;
        var checkOut = checkIn.AddMinutes(negativeDuration);
        Assert.Throws<ArgumentException>(() => _calculator.CalculateFee(vehicleType, membership, checkIn, checkOut, isLostTicket, isHoliday));
    }

    [Property]
    public void LostTicketPenalty_AppliesCorrectly(VehicleType vehicleType, MembershipTier membership, DateTime checkIn, int durationMinutes, bool isHoliday)
    {
        durationMinutes = Math.Abs(durationMinutes % 500000);
        if (durationMinutes <= 30) durationMinutes = 31;
        
        var checkOut = checkIn.AddMinutes(durationMinutes);
        
        var resultWithoutPenalty = _calculator.CalculateFee(vehicleType, membership, checkIn, checkOut, false, isHoliday);
        var resultWithPenalty = _calculator.CalculateFee(vehicleType, membership, checkIn, checkOut, true, isHoliday);
        
        Assert.Equal(resultWithoutPenalty.TotalFee + 20000m, resultWithPenalty.TotalFee);
    }

    [Property]
    public void BaseFee_IsMonotonicallyIncreasing(VehicleType vehicleType, int duration1, int duration2)
    {
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
        duration1 = Math.Abs(duration1 % (10 * 60));
        duration2 = Math.Abs(duration2 % (10 * 60));
        
        if (duration1 > duration2) 
        {
            var temp = duration1;
            duration1 = duration2;
            duration2 = temp;
        }
        
        var checkOut1 = checkIn.AddMinutes(duration1);
        var checkOut2 = checkIn.AddMinutes(duration2);
        
        var result1 = _calculator.CalculateFee(vehicleType, MembershipTier.Guest, checkIn, checkOut1, false, false);
        var result2 = _calculator.CalculateFee(vehicleType, MembershipTier.Guest, checkIn, checkOut2, false, false);
        
        Assert.True(result1.TotalFee <= result2.TotalFee);
    }
    #endregion
}
