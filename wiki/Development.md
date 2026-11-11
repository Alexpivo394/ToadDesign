# Разработка

Нужны .NET 10 SDK и .NET Framework 4.7 Targeting Pack (ставится с Developer Pack 4.8); IDE — JetBrains Rider или Visual Studio.

| Папка | Что там |
|---|---|
| `src/ToadDesign` | Библиотека |
| `src/ToadDesign.Gallery` | Галерея: все контролы во всех состояниях и обеих темах, окна-образцы |
| `tests/ToadDesign.Tests` | Проверки без Revit |
| `tools/IconGen` | Генератор иконок из пакета `@tabler/icons` |
| `wiki` | Вики; публикуется в GitHub Wiki при изменении в `main` |

## Галерея

```powershell
dotnet run --project src/ToadDesign.Gallery -f net8.0-windows
```

Снимки всех страниц в PNG, без показа окон на экране:

```powershell
dotnet run --project src/ToadDesign.Gallery -f net8.0-windows -- --snap out/snapshots
```

Картинки вики — эти снимки.

## Проверки

```bash
dotnet build ToadDesign.slnx
sh tests/run.sh
```

`run.sh` гоняет проверки под `net47` и `net8.0-windows`, на обычной сборке и на вшитой ILRepack. Окна при этом создаются на отдельном невидимом рабочем столе Windows и на экране не появляются. Прилипание окна к краю (`Win+стрелка`) проверяется только на видимом столе: `--visible`.

## Иконки

Список иконок и их данные генерируются из `tabler-nodes-outline.json` пакета `@tabler/icons`:

```powershell
dotnet run --project tools/IconGen -- <путь>/tabler-nodes-outline.json src/ToadDesign/Icons
```

`src/ToadDesign/Icons/symbols.txt` хранит порядок: новые иконки только дописываются в конец, значения `Symbol` не сдвигаются.

## Выпуск

Тег — версия без префикса (`1.0.0`). Workflow `PublishRelease` собирает пакет, публикует его на nuget.org (Trusted Publishing) и создаёт релиз на GitHub.
