using Reqnroll;
using System.Net;
using Xunit;

namespace MyAPI.BddTests.StepDefinitions;

[Binding]
public class HealthCheckSteps
{
    private readonly HttpClient _httpClient;
    private HttpResponseMessage? _response;

    public HealthCheckSteps()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
    }

    [Given("the API is running")]
    public void GivenTheApiIsRunning()
    {
        // API availability can be verified here
    }

    [When("I call the health endpoint")]
    public async Task WhenICallTheHealthEndpoint()
    {
        _response = await _httpClient.GetAsync("/health");
    }

    [Then("the response status code should be {int}")]
    public void ThenTheResponseStatusCodeShouldBe(int statusCode)
    {
        Assert.NotNull(_response);

        Assert.Equal(
            (HttpStatusCode)statusCode,
            _response.StatusCode);
    }
}