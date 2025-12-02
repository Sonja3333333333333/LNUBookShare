using System.Diagnostics.CodeAnalysis;

// Вимикає вимогу писати заголовок з ліцензією у кожному файлі
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File must have header", Justification = "Student Project")]

// Вимикає вимогу писати документацію (/// summary) до кожного методу
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements must be documented", Justification = "Student Project")]

// Вимикає вимогу документувати partial класи (SA1601)
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "Student Project")]

// --- ЧИТАБЕЛЬНІСТЬ ---
// Вимикає вимогу писати this. (SA1101)
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "Preference")]
// Дозволяє змінні з підкресленням типу _context (SA1309 - дуже важливо для сучасного коду!)
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:Field names should not begin with underscore", Justification = "Modern convention")]
// Дозволяє писати usings ЗОВНІ namespace (SA1200 - це стандарт Visual Studio, а StyleCop свариться дарма)
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directive should appear within a namespace declaration", Justification = "Standard convention")]