![Frame 39263](https://github.com/user-attachments/assets/d69fae32-4339-4239-8950-133a89763a82)

Gml (GamerVII Minecraft Launcher)
=======

[English](README.en.md)

Удобный лаунчер Minecraft для запуска игры и управления настройками. Интуитивный интерфейс и базовые возможности для комфортной игры.

## Шаблоны дизайна (для владельца проекта)

Игрок **не выбирает** тему. Шаблон задаётся при сборке — в **Rider** или в **панели Gml**.

| Где собираете | Как задать бренд |
|---------------|------------------|
| **Rider / CLI** | `Themes/my-brand` + `Themes/active.json` → `{ "theme": "my-brand" }` |
| **Панель** | Папка на сервере `{InstallationDirectory}/LauncherBrand/` (см. docs) — подставляется при скачивании и перед compile |

Стандарт: `src/Gml.Launcher/Themes/default`.

Полная инструкция: **[docs/THEMES.md](docs/THEMES.md)**.

> Бренд только в шаблоне (`Themes/` / `LauncherBrand`), не в `Views/`.

## Установка

### Требования

Перед установкой убедитесь, что установлены:

- **.NET 8.0 SDK** — [официальный сайт Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0) или менеджер пакетов вашей ОС.
- **Git** — [сайт Git](https://git-scm.com/downloads) или менеджер пакетов.

### Шаги установки

1. **Клонирование репозитория**  
   В терминале:
   ```bash
   git clone --recursive https://github.com/Nik497926/Gml.Launcher.git
   cd Gml.Launcher
   ```

2. **Сборка проекта**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Запуск лаунчера**
   ```bash
   dotnet run --project path/to/your/project
   ```

### Устранение неполадок

- Проверьте, что .NET SDK установлен корректно и доступен в PATH.
- Убедитесь, что Git установлен и доступен в терминале.
- Проверьте права на сборку и запуск приложения.

Дополнительная помощь — в issue tracker проекта или на форумах сообщества.

### Обновление

В каталоге проекта:

```bash
git pull origin main
git submodule update --recursive --remote
dotnet build
```

Затем снова запустите лаунчер командой выше.

### Удаление

Удалите каталог проекта:

```bash
rm -rf minecraft-launcher
```
