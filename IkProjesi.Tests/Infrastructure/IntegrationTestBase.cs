using Xunit;

namespace IkProjesi.Tests.Infrastructure;

/// <summary>
/// Tum entegrasyon testleri ayni veritabanini paylastigi icin tek koleksiyonda
/// ve sirayla calisirlar. Paralel kosum veritabani cakismasina yol acardi.
/// </summary>
[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFixture> { }

public class ApiFixture : IDisposable
{
    public TestWebApplicationFactory Factory { get; }

    public ApiFixture()
    {
        Factory = new TestWebApplicationFactory();
        Factory.ResetDatabase();
    }

    /// <summary>Her test kendi temiz veritabaniyla baslasin diye cagrilir.</summary>
    public void Reset() => Factory.ResetDatabase();

    public ApiClient NewClient() => new(Factory.CreateClient());

    public async Task<ApiClient> IkClientAsync() =>
        await NewClient().LoginAsAsync(TestUsers.IkEmail, TestUsers.IkPassword);

    public async Task<ApiClient> AdminClientAsync() =>
        await NewClient().LoginAsAsync(TestUsers.AdminEmail, TestUsers.AdminPassword);

    public async Task<ApiClient> CalisanClientAsync() =>
        await NewClient().LoginAsAsync(TestUsers.CalisanEmail, TestUsers.CalisanPassword);

    public async Task<ApiClient> MeslektasClientAsync() =>
        await NewClient().LoginAsAsync(TestUsers.MeslektasEmail, TestUsers.MeslektasPassword);

    public void Dispose() => Factory.Dispose();
}

[Collection("Api")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly ApiFixture Fixture;

    protected IntegrationTestBase(ApiFixture fixture)
    {
        Fixture = fixture;
    }

    public Task InitializeAsync()
    {
        // Testler birbirinin verisini gormesin diye her testten once sifirlanir.
        Fixture.Reset();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
