# SmartPark Assignment - 1-Night Speedrun Plan

This guide outlines exactly what to do step-by-step to finish the assignment in 2-4 hours tonight using AI.

## Phase 1: Setup & Planning (30 mins)

1.  **GitHub Setup:**
    *   Create a private repo: `automate-test-[class]-[name]`.
    *   Add `hangsopheak` as a collaborator.
    *   Clone it and copy the provided assignment code into your local repo.
2.  **Report - Part 1 (Crucial - Commit this first):**
    *   Open your report template (Word/PDF).
    *   Fill out the **Test Scenario Matrix** (Ask AI for \"10 test scenarios for ParkingFeeCalculator\").
    *   Define **5 Property-Based Tests** (Ask AI for \"5 property rules for ParkingFeeCalculator\").
    *   Save and **Git Commit** the report. *Do not write code before this commit.*

## Phase 2: TDD the `ParkingFeeCalculator` (1.5 - 2 Hours)

*You must do these steps in a loop. Do NOT write all tests at once.*

### The Loop for each of the 10 scenarios:
1.  **[RED] Step:**
    *   Ask AI: *\"Write the next failing xUnit test for [Scenario X] in ParkingFeeCalculatorTests.cs\"*
    *   Paste test into IDE. Run it. Ensure it fails.
    *   `git commit -m \"[RED] Add test for [Scenario X]\"`
2.  **[GREEN] Step:**
    *   Ask AI: *\"Write minimal code in ParkingFeeCalculator.cs to make this test pass\"*
    *   Paste code. Run tests. Ensure it passes.
    *   `git commit -m \"[GREEN] Implemented logic for [Scenario X]\"`
3.  **[REFACTOR] Step (Optional):**
    *   Clean up the code if needed.
    *   `git commit -m \"[REFACTOR] Cleaned up code for [Scenario X]\"`

*Repeat this loop until all 10 business rules from the specification are implemented.*

## Phase 3: Mocking & Property-Based Tests (1 Hour)

1.  **Moq Tests (`ParkingSessionManagerTests.cs`):**
    *   Ask AI: *\"Write the Moq tests for CheckInAsync and CheckOutAsync in ParkingSessionManagerTests.cs based on the spec inputs.\"*
    *   Paste and verify tests run successfully.
2.  **FsCheck Tests (`ParkingFeeCalculatorTests.cs`):**
    *   Ask AI: *\"Generate 5 Property-based tests using FsCheck for the ParkingFeeCalculator based on the spec.\"*
    *   Paste and ensure they pass.

## Phase 4: Integration Tests & Finalization (30 Mins)

1.  **Integration Tests (`ParkingFlowIntegrationTests.cs`):**
    *   Ask AI: *\"Write an End-to-End integration test for ParkingFlowIntegrationTests.cs that checks in and checks out.\"*
2.  **Final Report:**
    *   Run all tests. Take a screenshot of the \"All Tests Passed\" window with green checkmarks.
    *   Paste screenshots into your report.
    *   Write a short reflection.
3.  **Final Push:**
    *   Commit all final changes.
    *   `git push origin main`

## AI Prompts to Copy/Paste Later:
*   *\"Give me 10 test scenarios for the Test Scenario Matrix based on the SmartPark assignment specs.\"*
*   *\"Write a failing unit test for the Grace Period rule in ParkingFeeCalculatorTests.cs.\"*
*   *\"Write the Mock setups and tests for ParkingSessionManager.CheckInAsync.\"*