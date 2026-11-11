# Плагины Revit

ToadDesign сделана для окон внутри Revit: она не трогает `Application.Current`, не меняет интерфейс Revit и чужих плагинов и не зависит от них. Работает рядом с окнами на других библиотеках в том же сеансе Revit.

## Цели сборки

| Revit | .NET | Сборка пакета |
|---|---|---|
| 2020 | .NET Framework 4.7 | `net47` |
| 2021–2024 | .NET Framework 4.8 | `net47` |
| 2025–2026 | .NET 8 | `net8.0-windows` |
| 2027 | .NET 10 | `net8.0-windows` |

## Окна в отдельных потоках

Окно прогресса и другие окна в своём потоке работают как обычно: смена темы доходит до них в их потоке.

> [!WARNING]
> Перед концом потока, который показывал окна, закройте их или вызовите `Dispatcher.InvokeShutdown()`. В Revit 2025 и новее (.NET 8) поток, завершившийся с открытым окном, роняет весь Revit. Это поведение WPF, а не ToadDesign.

```csharp
var thread = new Thread(() =>
{
    var window = new ProgressWindow();
    window.Closed += (_, _) => Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
    window.Show();
    Dispatcher.Run();
});
thread.SetApartmentState(ApartmentState.STA);
thread.Start();
```

## Вшивание в сборку плагина (Revit 2020–2026)

До Revit 2027 все плагины грузятся в один контекст, и две разные версии одной библиотеки мешают друг другу. Вшейте ToadDesign в сборку плагина ILRepack и добавьте ключи `/copyattrs /allowMultiple`:

```
ILRepack /union /illink /parallel /copyattrs /allowMultiple /out:MyAddin.dll MyAddin.dll ToadDesign.dll
```

- Стили, шаблоны и иконки после вшивания находятся сами.
- Без `/copyattrs /allowMultiple` пропадает пространство имён `urn:toaddesign`: скомпилированный XAML продолжит работать, а XAML, разобранный во время работы через `XamlReader`, — нет.
- У каждой вшитой копии своя тема: два плагина переключают темы независимо.
