# DigitalMenuApi Testing Architecture

This document explains the tools and frameworks chosen for our automated unit testing suite and *why* we chose them.

## 1. xUnit (The Test Runner)
**What it is:** A free, open-source, community-focused unit testing tool for the .NET Framework.
**Why we chose it:** xUnit is the modern standard for .NET testing (heavily used by the ASP.NET Core team themselves). It enforces better testing practices (like not sharing state between tests accidentally) and runs tests in parallel by default, making the suite extremely fast.

## 2. Moq (The Mocking Framework)
**What it is:** A library that allows us to create "fake" versions of our dependencies (like the database or external APIs).
**Why we chose it:** When testing the `AuthService`, we don't want to actually connect to a real SQL database. Doing so would make tests slow, brittle, and require complex setup/teardown. Moq allows us to "mock" the `IUnitOfWork` repository. We can tell Moq: *"If the service asks for a user with ID 1, return this fake user object."* This lets us test the *business logic* in isolation from the database.

## 3. FluentAssertions (The Assertion Library)
**What it is:** A set of extension methods that allow us to specify the expected outcome of a test in a very natural, human-readable way.
**Why we chose it:** Standard xUnit assertions look like this: `Assert.Equal(expected, actual);`
FluentAssertions looks like this: `actual.Should().Be(expected);`
It makes our tests read like plain English, makes complex object comparisons trivial, and provides incredibly detailed error messages when a test fails.

## 4. EF Core InMemory Database (The Test Database)
**What it is:** A lightweight database provider for Entity Framework Core that stores data entirely in the computer's memory.
**Why we chose it:** While Moq is great for faking behavior, sometimes you want to write an integration test that actually exercises the full EF Core pipeline (LINQ queries, saving relationships) without spinning up a real SQL Server. The InMemory provider gives us a clean, empty database for every test run instantly.

---

### How to run the tests
From the root of the project, run:
```bash
dotnet test
```
