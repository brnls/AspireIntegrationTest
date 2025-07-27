using App;
using System.Text.Json;

namespace IntegrationTests;
public class PeopleTests : IntegrationTestBase
{
    public PeopleTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Test1()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var resp = JsonSerializer.Deserialize<Person[]>(await response.Content.ReadAsStringAsync());
        Assert.Equal(2, resp.Count());
    }
}
