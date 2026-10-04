using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Anitec.Platform.Tests.Unit.Support;

/// <summary>Localizer stub that returns the resource key, so tests can assert on it.</summary>
public class FakeLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name);

    public LocalizedString this[string name, params object[] arguments] => new(name, name);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}
