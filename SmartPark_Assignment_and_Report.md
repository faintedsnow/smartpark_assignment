# SmartPark Assignment Specification and Student Report Template

---

# Part A: Assignment Specification

**Assignment 2: SmartPark Testing Project**

(Individual)

Topic: **Unit Testing, TDD, Property-Based Testing & Integration Testing**

Timeline: **Weeks 5–8**

Deliverables: **Tested code (Private GitHub Repo) + Completed Report (PDF) + Demo**

Tech Stack: **.NET 10 • xUnit • Moq • FsCheck**

Currency: **KHR (Cambodian Riel)**

Source Code Repository:

https://github.com/hangsopheak/automate_test_assignment

# 1. What You Need to Do

A SmartPark parking application is provided to you on GitHub. Most of the code is already written. **Your job is to test it and implement one class (ParkingFeeCalculator) using TDD.** You will:

1.  **Fork or clone** the repository from the link on the cover page.

2.  **Write tests** for all business rules (unit tests, test doubles, TDD, property-based tests, integration tests).

3.  **Follow TDD** (Test-Driven Development) — write a test first, then make it pass, then clean up.

4.  **Fill in the Report Template** with your test plans, results, and reflections.

5.  **Submit** your GitHub link + completed report via Ms. Teams. Your Github must be **Private**.

> **Private GitHub repo:**

- Repository Name format: automate-test-\[class\]-\[name\]

- Add me <https://github.com/hangsopheak> as collaborator.

6.  **Demo** your work in a 1-on-1 session with the lecturer.

> **⚠️ Important:** The application source code is provided. You only write tests. The one exception is ParkingFeeCalculator.cs — this file is a stub (empty methods) that you implement yourself using TDD (write test first, then write the code to pass it).

# 2. Your Roadmap (Step by Step)

Follow these steps in order. Do not skip ahead.

## Step 1: Set Up Your Project

1.  Fork or clone the GitHub repository.

2.  Open SmartPark.slnx in Visual Studio 2022+ or JetBrains Rider.

3.  Restore NuGet packages and build the solution. Make sure there are no errors.

4.  Read this specification carefully. Understand the business rules before writing any test.

## Step 2: Plan Your Tests (Part 1 of the Report)

Before you write any code, fill in **Part 1 of the Report Template.** This includes:

- Test Scenario Matrix (inputs and expected outputs for every test)

- Property-Based Test Properties (what should always be true, minimum 5)

> **💡 Tip:** Commit your report first. Your git history should prove that you planned before you coded.

## Step 3: Write Tests Using TDD

For the **ParkingFeeCalculator** class, you must follow the TDD cycle. This class is provided as an **empty stub** (methods exist but have no logic). You build the implementation one test at a time:

1.  **RED:** Write one failing test. Commit with message like \[RED\] Add test for motorcycle fee.

2.  **GREEN:** Write the minimum code to make the test pass. Commit with \[GREEN\].

3.  **REFACTOR:** Clean up your code if needed. Commit with \[REFACTOR\].

4.  Repeat for the next business rule.

**Minimum 8 TDD commits are required.** A single commit with all tests = 0 points for TDD.

## Step 4: Write Test Doubles, PBT, and Integration Tests

After TDD, write the remaining tests:

- **ParkingSessionManagerTests.cs** — use Moq to replace all external dependencies with test doubles

- **Property-Based Tests** — use FsCheck to verify at least 5 properties

- **ParkingFlowIntegrationTests.cs** — wire real components together for end-to-end tests

## Step 5: Fill in the Rest of the Report

Paste your test outputs, fill in the traceability matrix, add your reflections, and complete the submission checklist.

## Step 6: Submit & Prepare for Demo

- Push all your code to GitHub.

- Submit the completed report as DOCX or PDF.

- Make sure your laptop builds and runs all tests for the 1-on-1 demo.

# 3. Project Structure

The repository contains the following folders and files:

| **Folder**                        | **Key Files**                                                                                                   |
| --------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| SmartPark.Core/Models/            | Vehicle.cs, ParkingTicket.cs, ParkingFeeResult.cs, VehicleType.cs, MembershipTier.cs                            |
| SmartPark.Core/Services/          | ParkingFeeCalculator.cs, ParkingSessionManager.cs                                                               |
| SmartPark.Core/Interfaces/        | IPaymentGateway.cs, INotificationService.cs, IMembershipService.cs, IParkingRepository.cs, IDateTimeProvider.cs |
| SmartPark.Tests/                  | ParkingFeeCalculatorTests.cs, ParkingSessionManagerTests.cs                                                     |
| SmartPark.Tests/IntegrationTests/ | ParkingFlowIntegrationTests.cs                                                                                  |

Your test files go in the **SmartPark.Tests/** folder. Do not modify the application source code **except** for ParkingFeeCalculator.cs, which you implement via TDD.

# 4. Business Rules — ParkingFeeCalculator

This is the core pricing engine that **you will implement using TDD.** It is provided as a stub (empty methods). It has **no external dependencies** — it takes inputs and returns a result. This makes it perfect for unit testing and property-based testing.

## Method Signature

> public ParkingFeeResult CalculateFee(
>
> VehicleType vehicleType, MembershipTier membership,
>
> DateTime checkIn, DateTime checkOut,
>
> bool isLostTicket = false, bool isHoliday = false)

## Pricing Constants

| **Constant**          | **Value**  | **Description**              |
| --------------------- | ---------- | ---------------------------- |
| MotorcycleRatePerHour | 500 KHR    | Motorcycle hourly rate       |
| CarRatePerHour        | 1,000 KHR  | Car hourly rate              |
| SuvRatePerHour        | 1,500 KHR  | SUV/Truck hourly rate        |
| GracePeriodMinutes    | 30         | Free parking window          |
| MotorcycleDailyCap    | 4,000 KHR  | Max daily fee for motorcycle |
| CarDailyCap           | 8,000 KHR  | Max daily fee for car        |
| SuvDailyCap           | 12,000 KHR | Max daily fee for SUV        |
| OvernightFlatFee      | 2,000 KHR  | Added if parked past 10 PM   |
| WeekendSurchargeRate  | 20%        | Weekend surcharge (Sat/Sun)  |
| HolidaySurchargeRate  | 50%        | Holiday surcharge            |
| LostTicketPenalty     | 20,000 KHR | Flat penalty for lost ticket |
| SilverDiscountRate    | 10%        | Silver member discount       |
| GoldDiscountRate      | 25%        | Gold member discount         |
| PlatinumDiscountRate  | 40%        | Platinum member discount     |

## How the Fee is Calculated

The calculation follows these steps **in order**:

1.  **Validate:** If checkOut is before checkIn, throw an ArgumentException.

2.  **Grace Period:** If total duration is 30 minutes or less, the fee is 0 (free parking).

3.  **Calculate Duration:** Billable hours = Math.Ceiling((totalMinutes − 30) / 60.0). Minimum is 1 hour.

4.  **Base Fee:** billableHours × hourlyRate. If the base fee exceeds the daily cap, use the daily cap instead.

5.  **Overnight:** If the parking session goes past 10:00 PM (22:00), add 2,000 KHR.

6.  **Surcharge:** If check-in is on Saturday or Sunday, add 20% to the base fee. If it is a holiday, add 50% instead. Weekend and holiday surcharges do NOT stack — holiday takes priority.

7.  **Membership Discount:** Apply the member’s discount rate to (baseFee + surcharge). Guest = 0%, Silver = 10%, Gold = 25%, Platinum = 40%.

8.  **Lost Ticket:** If isLostTicket is true, add 20,000 KHR. This penalty is NOT reduced by discounts.

9.  **Total:** baseFee + surcharge − discount + overnight + lostTicketPenalty. The total is never negative.

## Worked Examples

Study these examples carefully. They show exactly what the application should return.

### Grace Period

- Any vehicle, 30 minutes or less → 0 KHR (free)

- Any vehicle, 0 minutes → 0 KHR

- Car, 31 minutes → 1 hour billed = 1,000 KHR

### Basic Fees

- Motorcycle, 2 hours → 1,000 KHR

- Car, 3 hours → 3,000 KHR

- SUV, 1 hour → 1,500 KHR

### Duration Rounding (Always Round Up)

- 1h 1min past grace → 2 hours billed

- 2h 30min past grace → 3 hours billed

- Exactly 1h 30min total (= 60 min past grace) → 1 hour billed

### Daily Cap

- Motorcycle, 10 hours → capped at 4,000 KHR (not 5,000)

- Car, 12 hours → capped at 8,000 KHR (not 12,000)

- SUV, 24 hours → capped at 12,000 KHR

### Overnight Fee

- Car: check-in 8 PM, check-out 11 PM → base + 2,000 KHR

- Car: check-in 11 PM, check-out 6 AM → base + 2,000 KHR

- Car: check-in 8 AM, check-out 5 PM → no overnight fee

### Weekend Surcharge

- Car on Saturday, 2 hours → 2,000 + 400 = 2,400 KHR

- Motorcycle on Sunday, 1 hour → 500 + 100 = 600 KHR

- Car on Monday, 2 hours → no surcharge = 2,000 KHR

### Holiday Surcharge

- Car on holiday, 2 hours → 2,000 + 1,000 = 3,000 KHR

- Holiday that falls on a weekend → holiday surcharge only (no stacking)

### Membership Discounts

- Silver, Car, 2 hours → 2,000 − 200 = 1,800 KHR

- Gold, Car, 2 hours → 2,000 − 500 = 1,500 KHR

- Platinum, Car, 2 hours → 2,000 − 800 = 1,200 KHR

- Discount applies to (base fee + surcharge) combined

### Lost Ticket

- Car, lost ticket → normal fee + 20,000 KHR penalty

- Lost ticket penalty is NOT reduced by membership discount

- Lost ticket during grace period → 0 + 20,000 = 20,000 KHR

### Edge Cases

- checkOut before checkIn → throws ArgumentException

- checkIn equals checkOut (0 duration) → 0 KHR

# 5. Business Rules — ParkingSessionManager

This service controls the full parking flow (check-in, check-out, payment). It depends on external services. You will **mock these services** in your tests using Moq.

## Constructor Dependencies (What You Will Mock)

- **IPaymentGateway** — processes payments (external service)

- **INotificationService** — sends SMS/email receipts (external service)

- **IMembershipService** — looks up membership tier

- **IParkingRepository** — saves and retrieves tickets

- **IDateTimeProvider** — provides the current time (so you can control time in tests)

- **ParkingFeeCalculator** — the pricing engine (concrete class, do NOT mock this)

## CheckInAsync Flow

1.  Look up membership tier via IMembershipService.

2.  Check if this license plate already has an active ticket. If yes, throw InvalidOperationException.

3.  Create a new ParkingTicket with CheckInTime = IDateTimeProvider.Now.

4.  Save the ticket via IParkingRepository.SaveTicketAsync().

5.  Return the ticket.

## CheckOutAsync Flow

1.  **Find the ticket** by ID. If not found, throw KeyNotFoundException.

2.  **Check status.** If already checked out, throw InvalidOperationException.

3.  **Set times.** Set CheckOutTime and IsLostTicket.

4.  **Calculate fee** using ParkingFeeCalculator.

5.  **Process payment** via IPaymentGateway. If payment fails, throw an exception.

6.  **Update ticket** in the repository.

7.  **Send receipt** via INotificationService.

8.  **Return** the ParkingFeeResult.

> **🔑 Key Design Decision:** If SendReceiptAsync throws an error, the checkout should still succeed. The payment is already processed. This is called "graceful degradation" and you MUST write a test for it.

# 6. What Tests to Write

## Understanding the Two Types of Tests

Students often confuse **test doubles (mocking)** and **integration tests.** Here is the key difference:

| **Test Doubles (Mocking)**                                                                                          | **Integration Tests**                                                                                                                             |
| ------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| Test ONE class in isolation.                                                                                        | Test MULTIPLE real classes working together.                                                                                                      |
| All dependencies are replaced with fakes (mocks).                                                                   | Most dependencies are real. Only truly external services (e.g., payment gateway) are mocked.                                                      |
| You verify that the class calls the right methods in the right order.                                               | You verify the whole flow produces the correct end result.                                                                                        |
| Example: Test ParkingSessionManager where IPaymentGateway, INotificationService, IParkingRepository are ALL mocked. | Example: Test the full check-in → check-out flow using a real ParkingFeeCalculator + real InMemoryParkingRepository + real ParkingSessionManager. |
| File: ParkingSessionManagerTests.cs                                                                                 | File: ParkingFlowIntegrationTests.cs                                                                                                              |

## 6.1 Unit Tests — ParkingFeeCalculatorTests.cs

Test the pure pricing engine. Organize your tests with \#region blocks. **Minimum: 10 test scenarios.** Cover these areas:

- Basic fee calculation (all vehicle types)

- Grace period (0 min, 29 min, 30 min, 31 min)

- Duration rounding (always round up to next hour)

- Daily cap (verify fee never exceeds the cap)

- Overnight fee (before 10 PM, after 10 PM, no overnight)

- Weekend surcharge (Saturday, Sunday, weekday)

- Holiday surcharge (holiday, holiday on weekend)

- Membership discounts (all tiers)

- Lost ticket penalty

- Edge cases (checkOut before checkIn, zero duration)

**Naming convention:** Method_Scenario_ExpectedBehavior

_Example: CalculateFee_GracePeriod_30Minutes_ReturnsFree_

**Use \[Theory\] with \[InlineData\]** to test multiple inputs in one test method.

**Use the AAA pattern** (Arrange, Act, Assert) with blank lines between each section.

## 6.2 TDD — ParkingFeeCalculator (Red-Green-Refactor)

You must build ParkingFeeCalculator using TDD. The file is provided as an empty stub. **Minimum: 8 TDD commits.** Follow this cycle for each business rule:

5.  **RED:** Write one failing test. Commit with \[RED\].

6.  **GREEN:** Write the minimum code to make it pass. Commit with \[GREEN\].

7.  **REFACTOR:** Clean up if needed. Commit with \[REFACTOR\].

8.  Repeat for the next business rule.

Your git log should look similar to this:

> \[RED\] Add failing test for motorcycle hourly rate
>
> \[GREEN\] Implement motorcycle rate calculation
>
> \[RED\] Add failing test for grace period
>
> \[GREEN\] Implement grace period logic
>
> \[REFACTOR\] Extract rate lookup to switch expression
>
> \[RED\] Add failing test for daily cap
>
> \[GREEN\] Implement daily cap logic
>
> ...
>
> **⚠️ Warning:** A single commit with all tests and all code = 0 points for TDD. Your git history IS the evidence.

## 6.3 Test Doubles — ParkingSessionManagerTests.cs

Test the orchestration service using Moq. All external dependencies are replaced with test doubles (mocks/stubs). **Minimum: 5 test scenarios.** Cover:

- **CheckIn happy path:** ticket saved, membership looked up

- **Duplicate check-in:** throws exception, SaveTicketAsync NOT called

- **CheckOut happy path:** payment processed, receipt sent, ticket updated

- **Payment failure:** exception thrown, receipt and update NOT called

- **Notification failure:** checkout still succeeds (graceful degradation)

- **Ticket not found / already checked out:** throws correct exceptions

- **Interaction order:** payment happens BEFORE update, receipt AFTER payment

## 6.4 Property-Based Tests (FsCheck)

Write **at least 5** properties that must always be true. Required:

1.  **Fee is never negative.** For any valid inputs, TotalFee \>= 0.

2.  **Grace period is always free.** Duration ≤ 30 min → BaseFee == 0.

3.  **Longer stays cost more (or equal).** More time = higher or equal fee (up to daily cap).

4.  **Members pay ≤ guests.** Same conditions, member fee never exceeds guest fee.

5.  **Higher tier = higher discount.** Silver ≤ Gold ≤ Platinum discount.

6.  **Lost ticket adds exactly 20,000 KHR.** fee(lost) − fee(notLost) == 20,000.

7.  **Daily cap is respected.** BaseFee never exceeds the vehicle’s daily cap.

> **💡 Tip:** You will need to write a custom generator to create valid DateTime pairs (checkIn \< checkOut). Constrain checkIn to years 2024–2026 and checkOut to checkIn + 1 minute to checkIn + 48 hours.

## 6.5 Integration Tests — ParkingFlowIntegrationTests.cs

Wire real components together. Use a real ParkingFeeCalculator and InMemoryParkingRepository. Mock only external services (payment gateway returns true, notification is a no-op). **Minimum: 5 tests.**

Here are examples of what to test (you are not limited to these):

- Full flow: check in a car, check out after 2 hours, verify fee = 2,000 KHR

- Multiple vehicles: check in 3 cars, check out 1, verify 2 remain active

- Error recovery: duplicate check-in, failed payment leaves ticket active

- Edge-to-edge: grace period, overnight + weekend + Gold member, lost ticket with 0 base

## Minimum Requirements Summary

Your submission must meet **all** of the following minimums:

- **8** TDD commits (RED/GREEN/REFACTOR pattern in git history)

- **10** unit test scenarios for ParkingFeeCalculator

- **5** test double scenarios for ParkingSessionManager

- **5** property-based tests with FsCheck

- **5** integration test scenarios

- **Every** business rule mapped to at least one test in the traceability matrix

- **All** sections of the report filled in

# 7. Build & Run Commands

> \# Build the solution
>
> dotnet build SmartPark.slnx
>
> \# Run all tests
>
> dotnet test tests/SmartPark.Tests/SmartPark.Tests.csproj
>
> \# Run tests with code coverage
>
> dotnet test tests/SmartPark.Tests/SmartPark.Tests.csproj --collect:"XPlat Code Coverage"
>
> \# Run only ParkingFeeCalculator tests
>
> dotnet test --filter "FullyQualifiedName~ParkingFeeCalculatorTests"
>
> \# Run only integration tests
>
> dotnet test --filter "FullyQualifiedName~IntegrationTests"

# 8. Plagiarism Policy

**Academic integrity is strictly enforced.** Any work found to be copied will result in a score of 0 for both the student who copied and the student who provided the original. Each student must perform their own independent testing and write their own report.

> **🎯 About the Demo:** During the 1-on-1 demo, the lecturer will ask you to explain your testing decisions, walk through your code, and answer questions. Be ready to explain WHY you chose specific techniques for specific parts.

---

# Part B: Student Report Template

**SmartPark — Student Report**

(Individual)

Software Testing — Weeks 5–8 Assignment

**How to Read This Template**

|     |                                                                                            |
| --- | ------------------------------------------------------------------------------------------ |
|     | **Green cells** = sample data (provided for you as an example). Replace or continue below. |
|     | **Yellow cells** = your work goes here. Fill these in.                                     |

**⚠️ Fill in every section. Incomplete sections will lose marks.**

Submit this report along with your GitHub repository link.

Student Information

|                           |     |
| ------------------------- | --- |
| **Full Name**             |     |
| **Class / Group**         |     |
| **GitHub Repository URL** |     |

Part 1: Test Planning

_Complete this part BEFORE writing any test code. Your git history should show that planning came first._

1.1 Test Scenario Matrix

ParkingFeeCalculator Scenarios

📝 _List ALL your planned test scenarios with specific inputs and expected outputs. Minimum: 10 scenarios._

| **#** | **Scenario**                    | **Vehicle**  | **Duration**         | **Membership** | **Special**   | **Expected** | **Technique** |
| ----- | ------------------------------- | ------------ | -------------------- | -------------- | ------------- | ------------ | ------------- |
| _1_   | _Basic motorcycle fee_          | _Motorcycle_ | _2h_                 | _Guest_        | _None_        | _1,000 KHR_  | _Unit Test_   |
| _2_   | _Basic car fee_                 | _Car_        | _3h_                 | _Guest_        | _None_        | _3,000 KHR_  | _Unit Test_   |
| _3_   | _Basic SUV fee_                 | _SUV_        | _1h_                 | _Guest_        | _None_        | _1,500 KHR_  | _Unit Test_   |
| _4_   | _Grace period (≤ 30 min)_       | _Car_        | _30 min_             | _Guest_        | _None_        | _0 KHR_      | _Unit Test_   |
| _5_   | _Duration rounding (up)_        | _Car_        | _1h 1min_            | _Guest_        | _None_        | _2,000 KHR_  | _Unit Test_   |
| _6_   | _Daily Cap limit_               | _Motorcycle_ | _10h_                | _Guest_        | _None_        | _4,000 KHR_  | _Unit Test_   |
| _7_   | _Overnight fee_                 | _Car_        | _4h (crosses 10 PM)_ | _Guest_        | _None_        | _6,000 KHR_  | _Unit Test_   |
| _8_   | _Weekend surcharge_             | _Car_        | _2h_                 | _Guest_        | _Saturday_    | _2,400 KHR_  | _Unit Test_   |
| _9_   | _Holiday surcharge_             | _SUV_        | _2h_                 | _Guest_        | _Holiday_     | _4,500 KHR_  | _Unit Test_   |
| _10_  | _Membership discount (Gold)_    | _Car_        | _2h_                 | _Gold_         | _None_        | _1,500 KHR_  | _Unit Test_   |
| _11_  | _Lost Ticket penalty_           | _Car_        | _2h_                 | _Guest_        | _Lost Ticket_ | _22,000 KHR_ | _Unit Test_   |
| _12_  | _Invalid Dates (Exception)_     | _Car_        | _-1h (Invalid)_      | _Guest_        | _None_        | _Exception_  | _Unit Test_   |

_↑ This is just a sample row. Add as many rows as you need in your own report._

ParkingSessionManager Scenarios

📝 _Minimum: 5 scenarios._

| **#** | **Scenario** | **Test Double Setup** | **Expected Behavior** | **Verifications** |
| ----- | --------------------- | -------------------------------------------------------------------- | --------------------------------------------------------- | ------------------------------------- |
| _1_   | _Successful check-in_ | _Repo.GetActiveTicket returns null, DateTime stubbed_ | _Returns a new ParkingTicket with correct plate and time_ | _SaveTicketAsync called exactly once_ |
| _2_   | _Duplicate check-in_ | _Repo.GetActiveTicket returns existing ticket_ | _Throws InvalidOperationException_ | _SaveTicketAsync NOT called_ |
| _3_   | _Successful check-out_ | _Repo.GetTicketById returns ticket, Payment stub returns true_ | _Returns ParkingFeeResult, sets checkOut time on ticket_ | _UpdateTicketAsync called, SendReceiptAsync called_ |
| _4_   | _Payment failure_ | _Repo.GetTicketById returns ticket, Payment stub returns false_ | _Throws Exception_ | _UpdateTicketAsync NOT called, SendReceipt NOT called_ |
| _5_   | _Notification failure_ | _Notification stub throws exception during SendReceiptAsync_ | _Returns ParkingFeeResult successfully (graceful degradation)_ | _PaymentGateway called, UpdateTicketAsync called_ |

Integration Test Scenarios

📝 _Minimum: 5 scenarios._

| **#** | **Scenario** | **Components** | **Expected Result** |
| ----- | ---------------------------------- | ----------------------------------------------- | ----------------------------------------------- |
| _1_   | _Full parking flow (car, 2 hours)_ | _FeeCalculator + InMemoryRepo + SessionManager_ | _Fee = 2,000 KHR, ticket marked as checked out_ |
| _2_   | _Multiple vehicles concurrent_ | _FeeCalculator + InMemoryRepo + SessionManager_ | _Check in 3, check out 1, 2 remain active in repo_ |
| _3_   | _Error recovery (failed payment)_ | _FeeCalculator + InMemoryRepo + SessionManager_ | _Payment fails, ticket remains active in repo without checkOut_ |
| _4_   | _Edge-to-Edge (Overnight + Weekend + Gold)_ | _FeeCalculator + InMemoryRepo + SessionManager_ | _Correct fee calculated including modifiers and discounts_ |
| _5_   | _Grace period integration_ | _FeeCalculator + InMemoryRepo + SessionManager_ | _Check out within 15 min results in 0 KHR fee_ |

1.2 Property-Based Test Properties

📝 _List the properties you will verify with FsCheck. Explain WHY each property should always be true, no matter what inputs are generated. Minimum: 5 properties._

| **#** | **Property** | **Why It Must Always Hold** |
| ----- | ----------------------------- | ----------------------------------------------------------------------------------------------- |
| _1_   | _Fee is never negative_ | _A parking fee cannot be negative because you cannot owe a customer money for parking._ |
| _2_   | _Grace period is always free_ | _The business rule says parking ≤30 min costs 0 KHR, regardless of vehicle type or membership._ |
| _3_   | _Members pay less than or equal to guests_ | _Members have discount rates (10%, 25%, 40%), so their fee should never exceed a guest's._ |
| _4_   | _Lost ticket adds exactly 20,000 penalty_ | _The penalty is not discounted and should exactly increase the total by 20,000 KHR._ |
| _5_   | _Daily cap is respected_ | _The base fee portion before discounts/surcharges should never exceed the vehicle's cap limit._ |

Part 2: TDD Evidence

2.1 TDD Commit Log

📝 _List your TDD commits in order. Each row = one commit. This must match your actual git history. You need at least 8 commits showing the RED → GREEN → REFACTOR pattern._

To get your commit hashes, run: git log --oneline

|        |                 |                               |                                                     |
| ------ | --------------- | ----------------------------- | --------------------------------------------------- |
| **\#** | **Commit Hash** | **Type (RED/GREEN/REFACTOR)** | **Description**                                     |
| _1_    | _1d9bbb7_       | _RED_                         | _Add failing test for basic motorcycle fee_         |
| _2_    | _2041da2_       | _GREEN_                       | _Implement motorcycle hourly rate to pass the test_ |
| _3_    | _0de7350_       | _RED_                         | _Add tests for Car, SUV, Grace Period, Rounding, and failing test for Daily Cap_ |
| _4_    | _fb2a52b_       | _GREEN_                       | _Implement daily cap logic_                         |
| _5_    | _6dfacbd_       | _RED_                         | _Add failing test for overnight fee_                |
| _6_    | _b628ec1_       | _GREEN_                       | _Implement overnight fee logic_                     |
| _7_    | _64c3ec6_       | _RED_                         | _Add failing test for weekend and holiday surcharges_ |
| _8_    | _434a210_       | _GREEN_                       | _Implement weekend and holiday surcharges_          |
| _9_    | _fb67a30_       | _RED_                         | _Add failing test for membership discounts_         |
| _10_   | _dd4671c_       | _GREEN_                       | _Implement membership discounts_                    |
| _11_   | _ee3edfe_       | _RED_                         | _Add failing test for lost ticket penalty_          |
| _12_   | _dcb2c0b_       | _GREEN_                       | _Implement lost ticket penalty_                     |
| _13_   | _64e577d_       | _REFACTOR_                    | _Fix duration rounding test expectation_            |

Part 3: Test Results

This part is where you show your test outputs. For each section, run the command shown, then copy the **entire output** and paste it into the gray box.

3.1 Unit Test Results

Run this command in your terminal:

dotnet test --filter "ParkingFeeCalculatorTests"

<table>
<colgroup>
<col style="width: 100%" />
</colgroup>
<tbody>
<tr class="odd">
<td><p><strong>✅ Example:</strong></p>
<blockquote>
<p>Passed! - Failed: 0, Passed: 15, Skipped: 0, Total: 15</p>
<p>✓ CalculateFee_GracePeriod_30Minutes_ReturnsFree [&lt; 1ms]</p>
<p>✓ CalculateFee_Motorcycle_2Hours_Returns1000 [&lt; 1ms]</p>
<p>✓ CalculateFee_Car_3Hours_Returns3000 [&lt; 1ms]</p>
<p>... (your output will be longer)</p>
</blockquote></td>
</tr>
</tbody>
</table>

| Test Run Successful. Total tests: 30, Passed: 30, Failed: 0, Skipped: 0 |
| ---------------------------------------- |
| ✓ CalculateFee_GracePeriod_30MinutesOrLess_ReturnsFree (4 cases) |
| ✓ CalculateFee_Motorcycle_2Hours_Returns1000 |
| ✓ CalculateFee_Car_3Hours_Returns3000 |
| ✓ CalculateFee_SUV_1Hour_Returns1500 |
| ✓ CalculateFee_DurationRounding_AlwaysRoundsUp (7 cases) |
| ✓ CalculateFee_DailyCap_Motorcycle_10Hours_CappedAt4000 |
| ✓ CalculateFee_Overnight_Car_4Hours_Returns6000 |
| ✓ CalculateFee_NoOvernight_Car_9Hours_Returns8000 |
| ✓ CalculateFee_WeekendSurcharge_Saturday_Car_2Hours_Returns2400 |
| ✓ CalculateFee_HolidaySurcharge_TakesPriorityOverWeekend |
| ✓ CalculateFee_MembershipDiscount_Gold_25PercentOff |
| ✓ CalculateFee_LostTicket_Car_2Hours_Returns22000 |
| ✓ CalculateFee_CheckOutBeforeCheckIn_ThrowsArgumentException |
| ✓ CalculateFee_ZeroDuration_ReturnsFree |
| ✓ CalculateFee_ZeroDuration_ReturnsFree_Duplicate |
| ✓ CalculateFee_ExactGraceBoundary_31Minutes_Returns1Hour |
| ✓ Fee_IsNeverNegative [FsCheck 100 tests] |
| ✓ GracePeriod_IsAlwaysZero [FsCheck 100 tests] |
| ✓ CheckOutBeforeCheckIn_ThrowsArgumentException [FsCheck 100 tests] |
| ✓ LostTicketPenalty_AppliesCorrectly [FsCheck 100 tests] |
| ✓ BaseFee_IsMonotonicallyIncreasing [FsCheck 100 tests] |

3.2 Test Doubles Results

Run: dotnet test --filter "ParkingSessionManagerTests"

| Test Run Successful. Total tests: 6, Passed: 6, Failed: 0, Skipped: 0 |
| ---------------------------------------- |
| ✓ CheckInAsync_Successful_SavesTicketAndLooksUpMembership [56 ms] |
| ✓ CheckInAsync_Duplicate_ThrowsInvalidOperationException_DoesNotSave |
| ✓ CheckInAsync_NewVehicle_LookUpMembership |
| ✓ CheckOutAsync_HappyPath_UpdatesTicketAndSendsReceipt [7 ms] |
| ✓ CheckOutAsync_PaymentFailure_ThrowsException_DoesNotUpdateOrSendReceipt |
| ✓ CheckOutAsync_NotificationFailure_StillSucceeds_GracefulDegradation |

3.3 Property-Based Test Results

Run your FsCheck tests. The output should show how many random inputs were tested.

<table>
<colgroup>
<col style="width: 100%" />
</colgroup>
<tbody>
<tr class="odd">
<td><p><strong>✅ Example:</strong></p>
<blockquote>
<p>✓ FeeIsNeverNegative [28ms]</p>
<p>Ok, passed 100 tests.</p>
<p>✓ GracePeriodIsAlwaysFree [15ms]</p>
<p>Ok, passed 100 tests.</p>
</blockquote></td>
</tr>
</tbody>
</table>

| Test Run Successful. Total PBT tests: 5, all passed (100 random inputs each) |
| --------------------------------------- |
| ✓ Fee_IsNeverNegative [13 ms] — Ok, passed 100 tests |
| ✓ GracePeriod_IsAlwaysZero [6 ms] — Ok, passed 100 tests |
| ✓ CheckOutBeforeCheckIn_ThrowsArgumentException [3 ms] — Ok, passed 100 tests |
| ✓ LostTicketPenalty_AppliesCorrectly [3 ms] — Ok, passed 100 tests |
| ✓ BaseFee_IsMonotonicallyIncreasing [77 ms] — Ok, passed 100 tests |

3.4 Integration Test Results

Run: dotnet test --filter "IntegrationTests"

| Test Run Successful. Total tests: 6, Passed: 6, Failed: 0, Skipped: 0 |
| ---------------------------------------- |
| ✓ FullFlow_CheckInAndCheckOut_CalculatesCorrectFee |
| ✓ FullFlow_ComplexScenario_ChecksInAndChecksOut_WithAllModifiers |
| ✓ MultipleVehicles_CheckIn3_CheckOut1_TwoRemainActive |
| ✓ ErrorRecovery_FailedPayment_TicketRemainsActive |
| ✓ ErrorRecovery_DuplicateCheckIn_ThrowsAndOriginalRemains |
| ✓ EdgeToEdge_GracePeriod_CheckOutWithin15Min_FreeParking |

3.5 Code Coverage Report

**What is code coverage?** It measures what percentage of your application code is actually executed when your tests run. For example, if your tests never trigger the "lost ticket" path, that code has 0% coverage. Higher coverage means your tests are more thorough.

Run this command to generate coverage:

dotnet test --collect:"XPlat Code Coverage"

The output will show a coverage report file path. Open it and fill in the numbers below.

|                       |                   |                     |
| --------------------- | ----------------- | ------------------- |
| **Class**             | **Line Coverage** | **Branch Coverage** |
| ParkingFeeCalculator  | 81.25%            | 90.47%              |
| ParkingSessionManager | 96.72%            | 75.00%              |
| Overall               | 72.11%            | 88.46%              |

3.6 Full Test Summary

Finally, run ALL tests at once and paste the full output:

dotnet test

| Test Run Summary |
| ---------------- |
| Determining projects to restore... <br/> All projects are up-to-date for restore. <br/> SmartPark.Core -> D:\automate_test_assignment-main\automate_test_assignment-main\src\SmartPark.Core\bin\Debug\net10.0\SmartPark.Core.dll <br/> SmartPark.Tests -> D:\automate_test_assignment-main\automate_test_assignment-main\tests\SmartPark.Tests\bin\Debug\net10.0\SmartPark.Tests.dll <br/> Test run for D:\automate_test_assignment-main\automate_test_assignment-main\tests\SmartPark.Tests\bin\Debug\net10.0\SmartPark.Tests.dll (.NETCoreApp,Version=v10.0) <br/> A total of 1 test files matched the specified pattern. <br/> Passed!  - Failed:     0, Passed:    42, Skipped:     0, Total:    42, Duration: 149 ms - SmartPark.Tests.dll (net10.0) |

Fill in the counts:

| **Total Tests** | **Passed** | **Failed** | **Skipped** |
| --------------- | ---------- | ---------- | ----------- |
| **42**          | 42         | 0          | 0           |

Part 4: Traceability Matrix

**What is this?** A traceability matrix connects each business rule to the test(s) that prove it works. This shows the lecturer that every rule has been tested — nothing is missed.

📝 _For each business rule in the spec, write which test method(s) verify it. Every rule must have at least one test._

| **Business Rule** | **Test Method Name(s)** | **Test File** | **Pass/Fail** |
| ----------------- | ----------------------- | ------------- | ------------- |
| **Grace Period** (≤30 min = free) | `CalculateFee_GracePeriod_30MinutesOrLess_ReturnsFree`, `GracePeriod_IsAlwaysZero` (PBT), `EdgeToEdge_GracePeriod_CheckOutWithin15Min_FreeParking` | `ParkingFeeCalculatorTests.cs`, `ParkingFlowIntegrationTests.cs` | Pass |
| **Motorcycle Rate** (500 KHR/hr) | `CalculateFee_Motorcycle_2Hours_Returns1000` | `ParkingFeeCalculatorTests.cs` | Pass |
| **Car Rate** (1000 KHR/hr) | `CalculateFee_Car_3Hours_Returns3000` | `ParkingFeeCalculatorTests.cs` | Pass |
| **SUV Rate** (1500 KHR/hr) | `CalculateFee_SUV_1Hour_Returns1500` | `ParkingFeeCalculatorTests.cs` | Pass |
| **Duration Rounding** (Fractional hours round up) | `CalculateFee_DurationRounding_AlwaysRoundsUp`, `CalculateFee_ExactGraceBoundary_31Minutes_Returns1Hour` | `ParkingFeeCalculatorTests.cs` | Pass |
| **Daily Cap** (Motorcycle: 4k, Car: 10k, SUV: 15k) | `CalculateFee_DailyCap_Motorcycle_10Hours_CappedAt4000` | `ParkingFeeCalculatorTests.cs` | Pass |
| **Overnight Fee** (+2k for 10 PM - 6 AM) | `CalculateFee_Overnight_Car_4Hours_Returns6000`, `CalculateFee_NoOvernight_Car_9Hours_Returns8000` | `ParkingFeeCalculatorTests.cs` | Pass |
| **Weekend Surcharge** (+20% Sat/Sun) | `CalculateFee_WeekendSurcharge_Saturday_Car_2Hours_Returns2400` | `ParkingFeeCalculatorTests.cs` | Pass |
| **Holiday Surcharge** (+50%, takes priority) | `CalculateFee_HolidaySurcharge_TakesPriorityOverWeekend` | `ParkingFeeCalculatorTests.cs` | Pass |
| **Membership Discount** (Gold 25%, etc) | `CalculateFee_MembershipDiscount_Gold_25PercentOff` | `ParkingFeeCalculatorTests.cs` | Pass |
| **Lost Ticket Penalty** (+20k flat penalty) | `CalculateFee_LostTicket_Car_2Hours_Returns22000`, `LostTicketPenalty_AppliesCorrectly` (PBT) | `ParkingFeeCalculatorTests.cs` | Pass |
| **Check-Out before Check-In** (Throws validation err) | `CalculateFee_CheckOutBeforeCheckIn_ThrowsArgumentException`, `CheckOutBeforeCheckIn_ThrowsArgumentException` (PBT) | `ParkingFeeCalculatorTests.cs` | Pass |
| **Check-In Flow** (Saves ticket, looks up membership) | `CheckInAsync_NewVehicle_LookUpMembership`, `CheckInAsync_Successful_SavesTicketAndLooksUpMembership` | `ParkingSessionManagerTests.cs` | Pass |
| **Duplicate Check-In** (Throws exception, original active) | `CheckInAsync_Duplicate_ThrowsInvalidOperationException_DoesNotSave`, `ErrorRecovery_DuplicateCheckIn_ThrowsAndOriginalRemains` | `ParkingSessionManagerTests.cs`, `ParkingFlowIntegrationTests.cs` | Pass |
| **Check-Out & Payment** (Updates ticket, sends receipt) | `CheckOutAsync_HappyPath_UpdatesTicketAndSendsReceipt` | `ParkingSessionManagerTests.cs` | Pass |
| **Payment Failure** (Throws exception, ticket stays active) | `CheckOutAsync_PaymentFailure_ThrowsException_DoesNotUpdateOrSendReceipt`, `ErrorRecovery_FailedPayment_TicketRemainsActive` | `ParkingSessionManagerTests.cs`, `ParkingFlowIntegrationTests.cs` | Pass |
| **Notification Failure** (Graceful degradation, completes checkout) | `CheckOutAsync_NotificationFailure_StillSucceeds_GracefulDegradation` | `ParkingSessionManagerTests.cs` | Pass |

Submission Checklist

**Tick each item before submitting:**

- [x] Git history shows TDD commits (minimum 8 RED/GREEN/REFACTOR commits)

- [x] All tests pass (dotnet test output included above)

- [x] At least 10 unit test scenarios for ParkingFeeCalculator

- [x] At least 5 test double scenarios for ParkingSessionManager

- [x] At least 5 property-based tests written with FsCheck

- [x] At least 5 integration test scenarios

- [x] Traceability matrix is complete (every business rule has at least one test)

- [x] Code coverage report included

- [x] Laptop ready for 1-on-1 demo (project builds, all tests pass)

- [x] All sections of this report are filled in
