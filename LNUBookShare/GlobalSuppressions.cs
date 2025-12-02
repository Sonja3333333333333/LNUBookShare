using System.Diagnostics.CodeAnalysis;

// Вимикає вимогу писати заголовок з ліцензією у кожному файлі
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File must have header", Justification = "Student Project")]

// Вимикає вимогу писати документацію (/// summary) до кожного методу
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements must be documented", Justification = "Student Project")]

// Вимикає вимогу писати this. перед змінними (this.Name)
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "Preference")]
