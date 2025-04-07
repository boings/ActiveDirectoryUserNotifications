using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace CwrStatusChecker.Tests
{
    public class TestConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string> _data = new()
        {
            { "LdapEndpoints:0", "localhost:389" }
        };

        public string? this[string key]
        {
            get => _data.TryGetValue(key, out var value) ? value : null;
            set => _data[key] = value ?? string.Empty;
        }

        public IConfigurationSection GetSection(string key)
        {
            return new TestConfigurationSection(key, this);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _data.Keys
                .Select(k => k.Split(':')[0])
                .Distinct()
                .Select(k => new TestConfigurationSection(k, this));
        }

        public IChangeToken GetReloadToken()
        {
            return new TestChangeToken();
        }

        private class TestConfigurationSection : IConfigurationSection
        {
            private readonly string _key;
            private readonly TestConfiguration _parent;

            public TestConfigurationSection(string key, TestConfiguration parent)
            {
                _key = key;
                _parent = parent;
            }

            public string this[string key]
            {
                get => _parent[$"{_key}:{key}"] ?? string.Empty;
                set => _parent[$"{_key}:{key}"] = value;
            }

            public string Key => _key;
            public string Path => _key;
            public string? Value { get; set; }

            public IConfigurationSection GetSection(string key)
            {
                return new TestConfigurationSection($"{_key}:{key}", _parent);
            }

            public IEnumerable<IConfigurationSection> GetChildren()
            {
                return _parent._data.Keys
                    .Where(k => k.StartsWith(_key + ":"))
                    .Select(k => k.Substring(_key.Length + 1))
                    .Select(k => new TestConfigurationSection($"{_key}:{k}", _parent));
            }

            public IChangeToken GetReloadToken()
            {
                return new TestChangeToken();
            }
        }

        private class TestChangeToken : IChangeToken
        {
            public bool HasChanged => false;
            public bool ActiveChangeCallbacks => false;
            public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
            {
                return new TestDisposable();
            }

            private class TestDisposable : IDisposable
            {
                public void Dispose() { }
            }
        }
    }
} 