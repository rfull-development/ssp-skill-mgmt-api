// Copyright (c) 2024 RFull Development
// This source code is managed under the MIT license. See LICENSE in the project root.
using Api.Client;
using Api.Client.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace ComponentTest.Basic
{
    [TestClass]
    [TestCategory("Component")]
    public class TagTest
    {
        private static IHttpClientFactory _httpClientFactory = null!;
        private ApiClient _client = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            ServiceCollection services = new();
            services.AddHttpClient();
            var provider = services.BuildServiceProvider();
            _httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            AnonymousAuthenticationProvider provider = new();
            HttpClientRequestAdapter adapter = new(authenticationProvider: provider, httpClient: httpClient);
            _client = new(adapter);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public async Task CreateTag()
        {
            try
            {
                TagCreateRequest request = new()
                {
                    Name = $"Test Tag-{Guid.NewGuid()}"
                };
                var response = await _client.Tags.PostAsync(request);
                if (response is null)
                {
                    Assert.Fail("Failed to get response.");
                    return;
                }
                if (string.IsNullOrEmpty(response.Id))
                {
                    Assert.Fail(response.ToString());
                    return;
                }
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        public async Task CreateTag_Duplicate()
        {
            string name;
            try
            {
                TagCreateRequest request = new()
                {
                    Name = $"Test Tag-{Guid.NewGuid()}"
                };
                var response = await _client.Tags.PostAsync(request);
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                if (string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                name = request.Name;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            try
            {
                TagCreateRequest request = new()
                {
                    Name = name
                };
                await _client.Tags.PostAsync(request);
                Assert.Fail();
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task GetTagList()
        {
            try
            {
                var response = await _client.Tags.GetAsTagListGetResponseAsync();
                if (response is null)
                {
                    Assert.Fail("Failed to get response.");
                    return;
                }
                if ((response.Count is not int count) ||
                    (response.Tags is not List<Tag> users))
                {
                    Assert.Fail(response.ToString());
                    return;
                }
                Assert.AreEqual(count, users.Count);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        public async Task GetTag()
        {
            string id;
            string name;
            try
            {
                TagCreateRequest request = new()
                {
                    Name = $"Test Tag-{Guid.NewGuid()}"
                };
                var response = await _client.Tags.PostAsync(request);
                if ((response is null) ||
                    string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                id = response.Id;
                name = request.Name;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Tags[id].GetAsTagGetResponseAsync();
                if (response is null)
                {
                    Assert.Fail("Failed to get response.");
                    return;
                }
                Assert.AreEqual(id, response.Id);
                Assert.AreEqual(name, response.Name);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        public async Task GetTag_Invalid()
        {
            try
            {
                await _client.Tags["Invalid"].GetAsTagGetResponseAsync();
                Assert.Fail();
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task UpdateTag()
        {
            string id;
            try
            {
                TagCreateRequest request = new()
                {
                    Name = $"Test Tag-{Guid.NewGuid()}"
                };
                var response = await _client.Tags.PostAsync(request);
                if ((response is null) ||
                    string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                id = response.Id;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            string name;
            try
            {
                TagUpdateRequest request = new()
                {
                    Name = $"Test Tag-{Guid.NewGuid()}"
                };
                await _client.Tags[id].PatchAsync(request);
                name = request.Name;
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Tags[id].GetAsTagGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(name, response.Name);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateTag_Overwrite()
        {
            string id;
            try
            {
                TagCreateRequest request = new()
                {
                    Name = $"Test Tag-{Guid.NewGuid()}"
                };
                var response = await _client.Tags.PostAsync(request);
                if ((response is null) ||
                    string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                id = response.Id;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            try
            {
                TagUpdateRequest request = new()
                {
                    Name = $"Test Tag-{Guid.NewGuid()}"
                };
                await _client.Tags[id].PatchAsync(request);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            string name;
            try
            {
                TagUpdateRequest request = new()
                {
                    Name = $"Test Tag-{Guid.NewGuid()}"
                };
                await _client.Tags[id].PatchAsync(request);
                name = request.Name;
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Tags[id].GetAsTagGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(name, response.Name);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateTag_Invalid()
        {
            try
            {
                TagUpdateRequest request = new()
                {
                    Name = $"Test Tag-{Guid.NewGuid()}"
                };
                await _client.Tags["Invalid"].PatchAsync(request);
                Assert.Fail();
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task DeleteTag()
        {
            string id;
            try
            {
                TagCreateRequest request = new()
                {
                    Name = $"Test Tag-{Guid.NewGuid()}"
                };
                var response = await _client.Tags.PostAsync(request);
                if ((response is null) ||
                    string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                id = response.Id;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            try
            {
                await _client.Tags[id].DeleteAsync();
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                await _client.Tags[id].GetAsTagGetResponseAsync();
                Assert.Fail();
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task DeleteTag_Invalid()
        {
            try
            {
                await _client.Tags["Invalid"].DeleteAsync();
                Assert.Fail();
            }
            catch
            {
            }
        }
    }
}
