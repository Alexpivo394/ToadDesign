# Начало работы

## Подключение

Пакет — `Alexpivo394.ToadDesign`.

```xml
<PackageReference Include="Alexpivo394.ToadDesign" Version="0.1.0-preview.2" />
```

> [!NOTE]
> Пока пакет раздаётся через локальную ленту NuGet, а не через nuget.org. Добавьте папку ленты в `nuget.config` рядом с решением:
>
> ```xml
> <configuration>
>   <packageSources>
>     <add key="local" value="..\_localfeed" />
>   </packageSources>
> </configuration>
> ```

Пространство имён XAML — `urn:toaddesign`, принятый префикс — `toad`. В C# — `using ToadDesign;`.

## Первое окно

Замените `Window` на `toad:ToadWindow` — и в разметке, и в базовом классе.

```xml
<toad:ToadWindow x:Class="MyAddin.Views.DashWindow"
                 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                 xmlns:toad="urn:toaddesign"
                 Title="Записать прочерк" TitleIcon="Minus"
                 Width="460" Height="470">
    <DockPanel Margin="20,6,20,18">
        <toad:TextBox DockPanel.Dock="Top" Placeholder="Поиск параметров..." Icon="Search" />
        <DockPanel DockPanel.Dock="Bottom" Margin="0,14,0,0" LastChildFill="False">
            <toad:Switch Content="Тёмная тема" IsChecked="{Binding IsDark}" />
            <toad:Button DockPanel.Dock="Right" Variant="Filled" Icon="Rocket" Content="Запуск"
                         Command="{Binding RunCommand}" />
        </DockPanel>
        <ListBox Margin="0,12,0,0" ItemsSource="{Binding Parameters}" />
    </DockPanel>
</toad:ToadWindow>
```

```csharp
public partial class DashWindow : ToadWindow
{
    public DashWindow() => InitializeComponent();
}
```

Окно само является **корнем темы**: внутри него всё оформлено, снаружи ничего не меняется.

## Панель или вложенный вид

Для `UserControl` и `Page` — например, закрепляемой панели Revit — отметьте корень:

```xml
<UserControl xmlns:toad="urn:toaddesign" toad:Theme.IsRoot="True">
    ...
</UserControl>
```

## Свои стили

`BasedOn` работает и по типу, и по ключу:

```xml
<UserControl.Resources>
    <Style TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
        <Setter Property="Margin" Value="0,0,8,0" />
    </Style>
    <Style x:Key="WideButton" TargetType="toad:Button" BasedOn="{StaticResource Toad.Style.ToadButton}">
        <Setter Property="MinWidth" Value="160" />
    </Style>
</UserControl.Resources>
```

Явный `<ResourceDictionary>` в ресурсах окна или панели тоже можно: тема подключится к нему сама.
