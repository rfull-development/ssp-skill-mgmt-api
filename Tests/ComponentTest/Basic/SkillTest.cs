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
    public class SkillTest
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
        public async Task CreateSkill()
        {
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                return;
            }
        }

        [TestMethod]
        public async Task CreateSkill_Duplicate()
        {
            string name;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
            catch (Exception ex)
            {
                Assert.Inconclusive(ex.Message);
                return;
            }

            try
            {
                SkillCreateRequest request = new()
                {
                    Name = name
                };
                await _client.Skills.PostAsync(request);
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task GetSkillList()
        {
            try
            {
                var response = await _client.Skills.GetAsSkillListGetResponseAsync();
                if (response is null)
                {
                    Assert.Fail("Failed to get response.");
                    return;
                }
                if ((response.Count is not int count) ||
                    (response.Skills is not List<Skill> users))
                {
                    Assert.Fail(response.ToString());
                    return;
                }
                Assert.AreEqual(count, users.Count);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task GetSkill()
        {
            string id;
            string name;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                var response = await _client.Skills[id].GetAsSkillGetResponseAsync();
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
                return;
            }
        }

        [TestMethod]
        public async Task GetSkill_InvalidId()
        {
            try
            {
                string id = "InvalidId";
                await _client.Skills[id].GetAsSkillGetResponseAsync();
                Assert.Fail();
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task UpdateSkill()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                SkillUpdateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                await _client.Skills[id].PatchAsync(request);
                name = request.Name;
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Skills[id].GetAsSkillGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(name, response.Name);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateSkill_Overwrite()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                SkillUpdateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                await _client.Skills[id].PatchAsync(request);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            string name;
            try
            {
                SkillUpdateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                await _client.Skills[id].PatchAsync(request);
                name = request.Name;
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Skills[id].GetAsSkillGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(name, response.Name);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateSkill_InvalidId()
        {
            try
            {
                string id = "InvalidId";
                SkillUpdateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                await _client.Skills[id].PatchAsync(request);
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task DeleteSkill()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                await _client.Skills[id].DeleteAsync();
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                await _client.Skills[id].GetAsSkillGetResponseAsync();
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task DeleteSkill_InvalidId()
        {
            try
            {
                string id = "InvalidId";
                await _client.Skills[id].DeleteAsync();
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task GetSkillTagList()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                var response = await _client.Skills[id].Tags.GetAsSkillTagListGetResponseAsync();
                if (response is null)
                {
                    Assert.Fail("Failed to get response.");
                    return;
                }
                Assert.AreEqual(0, response.Count);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateSkillTagList()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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

            List<SkillTag> exceptedTags;
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
                List<SkillTag> tags = [];
                SkillTag tag = new()
                {
                    Id = response.Id,
                    Name = request.Name
                };
                tags.Add(tag);
                exceptedTags = tags;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            try
            {
                List<string> tagIds = exceptedTags.Select(tag => tag.Id ?? throw new InvalidDataException()).ToList();
                SkillTagListUpdateRequest request = new()
                {
                    TagIds = tagIds
                };
                await _client.Skills[id].Tags.PutAsync(request);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Skills[id].Tags.GetAsSkillTagListGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(exceptedTags.Count, response.Count);
                var tags = response.Tags;
                if (tags is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                foreach (var tag in tags)
                {
                    bool found = exceptedTags.Any(exceptedTag => exceptedTag.Id == tag.Id);
                    Assert.IsTrue(found);
                }
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateSkillTagList_InvalidId()
        {
            try
            {
                string id = "InvalidId";
                List<string> tagIds = [];
                SkillTagListUpdateRequest request = new()
                {
                    TagIds = tagIds
                };
                await _client.Skills[id].Tags.PutAsync(request);
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task UpdateSkillTagList_InvalidTagId()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                List<string> tagIds = [
                    "InvalidId"
                ];
                SkillTagListUpdateRequest request = new()
                {
                    TagIds = tagIds
                };
                await _client.Skills[id].Tags.PutAsync(request);
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task DeleteSkillTagList()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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

            List<SkillTag> exceptedTags;
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
                    return;
                }
                List<SkillTag> tags = [];
                SkillTag tag = new()
                {
                    Id = response.Id,
                    Name = request.Name
                };
                tags.Add(tag);
                exceptedTags = tags;
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                List<string> tagIds = exceptedTags.Select(tag => tag.Id ?? throw new InvalidDataException()).ToList();
                SkillTagListUpdateRequest request = new()
                {
                    TagIds = tagIds
                };
                await _client.Skills[id].Tags.PutAsync(request);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            try
            {
                await _client.Skills[id].Tags.DeleteAsync();
            }
            catch
            {
                Assert.Fail();
                return;
            }

            try
            {
                var response = await _client.Skills[id].Tags.GetAsSkillTagListGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(0, response.Count);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task DeleteSkillTagList_InvalidId()
        {
            try
            {
                string id = "InvalidId";
                await _client.Skills[id].Tags.DeleteAsync();
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task GetSkillAliasList()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                var response = await _client.Skills[id].Aliases.GetAsSkillAliasListGetResponseAsync();
                if (response is null)
                {
                    Assert.Fail("Failed to get response.");
                    return;
                }
                Assert.AreEqual(0, response.Count);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateSkillAliasList()
        {
            List<string> exceptedAliases;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
                if ((response is null) ||
                    string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                string aliasId = response.Id;
                List<string> aliases = [];
                aliases.Add(aliasId);
                exceptedAliases = aliases;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                SkillAliasListUpdateRequest request = new()
                {
                    Aliases = exceptedAliases
                };
                await _client.Skills[id].Aliases.PutAsync(request);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Skills[id].Aliases.GetAsSkillAliasListGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(exceptedAliases.Count, response.Count);
                var aliases = response.Aliases;
                if (aliases is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                foreach (var alias in aliases)
                {
                    bool found = exceptedAliases.Any(exceptedAlias => exceptedAlias == alias);
                    Assert.IsTrue(found);
                }
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateSkillAliasList_InvalidId()
        {
            try
            {
                string id = "InvalidId";
                List<string> aliases = [];
                SkillAliasListUpdateRequest request = new()
                {
                    Aliases = aliases
                };
                await _client.Skills[id].Aliases.PutAsync(request);
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task UpdateSkillAliasList_InvalidAliasId()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                List<string> aliases = [];
                aliases.Add("InvalidId");
                SkillAliasListUpdateRequest request = new()
                {
                    Aliases = aliases
                };
                await _client.Skills[id].Aliases.PutAsync(request);
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task DeleteSkillAliasList()
        {
            List<string> exceptedAliases;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
                if ((response is null) ||
                    string.IsNullOrEmpty(response.Id))
                {
                    Assert.Inconclusive();
                    return;
                }
                string alias = response.Id;
                List<string> aliases = [];
                aliases.Add(alias);
                exceptedAliases = aliases;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                SkillAliasListUpdateRequest request = new()
                {
                    Aliases = exceptedAliases
                };
                await _client.Skills[id].Aliases.PutAsync(request);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                await _client.Skills[id].Aliases.DeleteAsync();
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Skills[id].Aliases.GetAsSkillAliasListGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(0, response.Count);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task DeleteSkillAliasList_InvalidId()
        {
            try
            {
                string id = "InvalidId";
                await _client.Skills[id].Aliases.DeleteAsync();
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task GetSkillLinkList()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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
                var response = await _client.Skills[id].Links.GetAsSkillLinkListGetResponseAsync();
                if (response is null)
                {
                    Assert.Fail("Failed to get response.");
                    return;
                }
                Assert.AreEqual(0, response.Count);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateSkillLinkList()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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

            List<SkillLink> exceptedLinks;
            try
            {
                List<SkillLink> links = [
                    new SkillLink
                    {
                        Title = $"Test Link-{Guid.NewGuid()}",
                        Url = "https://www.example.com"
                    }
                ];
                SkillLinkListUpdateRequest request = new()
                {
                    Links = links
                };
                await _client.Skills[id].Links.PutAsync(request);
                exceptedLinks = links;
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Skills[id].Links.GetAsSkillLinkListGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(exceptedLinks.Count, response.Count);
                var links = response.Links;
                if (links is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                foreach (var link in links)
                {
                    bool found = false;
                    foreach (var exceptedLink in exceptedLinks)
                    {
                        if (exceptedLink.Title == link.Title &&
                            exceptedLink.Url == link.Url)
                        {
                            found = true;
                            break;
                        }
                    }
                    Assert.IsTrue(found);
                }
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task UpdateSkillLinkList_InvalidId()
        {
            try
            {
                string id = "InvalidId";
                List<SkillLink> links = [
                    new SkillLink
                    {
                        Title = $"Test Link-{Guid.NewGuid()}",
                        Url = "https://www.example.com"
                    }
                ];
                SkillLinkListUpdateRequest request = new()
                {
                    Links = links
                };
                await _client.Skills[id].Links.PutAsync(request);
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }

        [TestMethod]
        public async Task DeleteSkillLinkList()
        {
            string id;
            try
            {
                SkillCreateRequest request = new()
                {
                    Name = $"Test Skill-{Guid.NewGuid()}"
                };
                var response = await _client.Skills.PostAsync(request);
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

            List<SkillLink> exceptedLinks;
            try
            {
                List<SkillLink> links = [
                    new SkillLink
                    {
                        Title = $"Test Link-{Guid.NewGuid()}",
                        Url = "https://www.example.com"
                    }
                ];
                SkillLinkListUpdateRequest request = new()
                {
                    Links = links
                };
                await _client.Skills[id].Links.PutAsync(request);
                exceptedLinks = links;
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }

            try
            {
                await _client.Skills[id].Links.DeleteAsync();
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                return;
            }

            try
            {
                var response = await _client.Skills[id].Links.GetAsSkillLinkListGetResponseAsync();
                if (response is null)
                {
                    Assert.Inconclusive();
                    return;
                }
                Assert.AreEqual(0, response.Count);
            }
            catch (Exception exception)
            {
                Assert.Inconclusive(exception.Message);
                return;
            }
        }

        [TestMethod]
        public async Task DeleteSkillLinkList_InvalidId()
        {
            try
            {
                string id = "InvalidId";
                await _client.Skills[id].Links.DeleteAsync();
                Assert.Fail();
                return;
            }
            catch
            {
            }
        }
    }
}
