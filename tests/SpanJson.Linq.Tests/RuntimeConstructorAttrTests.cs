using System;
using Xunit;

namespace SpanJson.Tests
{
    public class RuntimeConstructorAttrTests
    {
        static RuntimeConstructorAttrTests()
        {
            typeof(UserLoginInfo).AddRuntimeAttributes(new JsonConstructorAttribute());
            typeof(UserLoginInfoMultiCtors).AddRuntimeAttributes(new JsonConstructorAttribute(new Type[] { typeof(string), typeof(string), typeof(string) }));

            typeof(UserLoginInfoNoMatch).AddRuntimeAttributes(new JsonConstructorAttribute(nameof(UserLoginInfoNoMatch.LoginProvider), nameof(UserLoginInfoNoMatch.ProviderKey), nameof(UserLoginInfoNoMatch.ProviderDisplayName)));
            typeof(UserLoginInfoMultiCtorsNoMatch).AddRuntimeAttributes(new JsonConstructorAttribute(
                new string[] { nameof(UserLoginInfoMultiCtorsNoMatch.LoginProvider), nameof(UserLoginInfoMultiCtorsNoMatch.ProviderKey), nameof(UserLoginInfoMultiCtorsNoMatch.ProviderDisplayName) },
                new Type[] { typeof(string), typeof(string), typeof(string) }
                ));
        }

        [Fact]
        public void UserLoginInfoTest()
        {
            var info = new UserLoginInfo("a", "b", "c");
            var json = JsonSerializer.Generic.Utf16.Serialize(info);
            var newInfo = JsonSerializer.Generic.Utf16.Deserialize<UserLoginInfo>(json);
            Assert.Equal(info.LoginProvider, newInfo.LoginProvider);
            Assert.Equal(info.ProviderKey, newInfo.ProviderKey);
            Assert.Equal(info.ProviderDisplayName, newInfo.ProviderDisplayName);
        }

        [Fact]
        public void UserLoginInfoNoMatchTest()
        {
            var info = new UserLoginInfoNoMatch("a", "b", "c");
            var json = JsonSerializer.Generic.Utf16.Serialize(info);
            var newInfo = JsonSerializer.Generic.Utf16.Deserialize<UserLoginInfoNoMatch>(json);
            Assert.Equal(info.LoginProvider, newInfo.LoginProvider);
            Assert.Equal(info.ProviderKey, newInfo.ProviderKey);
            Assert.Equal(info.ProviderDisplayName, newInfo.ProviderDisplayName);
        }

        [Fact]
        public void UserLoginInfoMultiCtorsTest()
        {
            var info = new UserLoginInfoMultiCtors("a", "b", "c");
            var json = JsonSerializer.Generic.Utf16.Serialize(info);
            var newInfo = JsonSerializer.Generic.Utf16.Deserialize<UserLoginInfoMultiCtors>(json);
            Assert.Equal(info.LoginProvider, newInfo.LoginProvider);
            Assert.Equal(info.ProviderKey, newInfo.ProviderKey);
            Assert.Equal(info.ProviderDisplayName, newInfo.ProviderDisplayName);
        }

        [Fact]
        public void UserLoginInfoMultiCtorsNoMatchTest()
        {
            var info = new UserLoginInfoMultiCtorsNoMatch("a", "b", "c");
            var json = JsonSerializer.Generic.Utf16.Serialize(info);
            var newInfo = JsonSerializer.Generic.Utf16.Deserialize<UserLoginInfoMultiCtorsNoMatch>(json);
            Assert.Equal(info.LoginProvider, newInfo.LoginProvider);
            Assert.Equal(info.ProviderKey, newInfo.ProviderKey);
            Assert.Equal(info.ProviderDisplayName, newInfo.ProviderDisplayName);
        }

        public class UserLoginInfo
        {
            public UserLoginInfo(string loginProvider, string providerKey, string providerDisplayName)
            {
                LoginProvider = loginProvider;
                ProviderKey = providerKey;
                ProviderDisplayName = providerDisplayName;
            }

            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string ProviderDisplayName { get; set; }
        }

        public class UserLoginInfoNoMatch
        {
            public UserLoginInfoNoMatch(string loginProvider, string providerKey, string displayName)
            {
                LoginProvider = loginProvider;
                ProviderKey = providerKey;
                ProviderDisplayName = displayName;
            }

            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string ProviderDisplayName { get; set; }
        }

        public class UserLoginInfoMultiCtors
        {
            public UserLoginInfoMultiCtors(string loginProvider)
            {
                LoginProvider = loginProvider;
            }
            public UserLoginInfoMultiCtors(string loginProvider, string providerKey, string providerDisplayName)
            {
                LoginProvider = loginProvider;
                ProviderKey = providerKey;
                ProviderDisplayName = providerDisplayName;
            }

            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string ProviderDisplayName { get; set; }
        }

        public class UserLoginInfoMultiCtorsNoMatch
        {
            public UserLoginInfoMultiCtorsNoMatch(string loginProvider)
            {
                LoginProvider = loginProvider;
            }
            public UserLoginInfoMultiCtorsNoMatch(string loginProvider, string providerKey, string displayName)
            {
                LoginProvider = loginProvider;
                ProviderKey = providerKey;
                ProviderDisplayName = displayName;
            }

            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string ProviderDisplayName { get; set; }
        }
    }
}
