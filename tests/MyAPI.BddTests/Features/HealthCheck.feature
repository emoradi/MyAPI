Feature: Health Check

As an API consumer
I want to check the API health
So that I know the application is running

Scenario: API is healthy
    Given the API is running
    When I call the health endpoint
    Then the response status code should be 200