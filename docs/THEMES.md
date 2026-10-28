# Шаблоны лаунчера (white-label)

Система тем — для **владельца проекта**, не для игрока.

- Игрок **не выбирает** тему.
- Активный шаблон задаётся при сборке и попадает в дистрибутив.
- Стандарт Gml: `Themes/default`.
- Свой бренд: своя папка + `active.json` → у всех игроков только он.

Сборка бывает в **Rider** или в **панели Gml** — оба варианта поддерживаются.

---

## Как система выбирает тему

1. Рядом с exe читается `Themes/active.json`
2. Берётся `"theme": "имя-папки"`
3. Применяется `Themes/{имя}/theme.json`

```json
{ "theme": "my-brand" }
```

Нет файла / битый / нет папки → `default`.

---

## Вариант 1: сборка в Rider (или CLI)

1. Откройте решение лаунчера в Rider.
2. Перейдите в `src/Gml.Launcher/Themes/`.
3. Скопируйте `default` → `my-brand`.
4. Правите `my-brand/theme.json`, положите `logo.svg` / `logo.png`.
5. В `Themes/active.json` укажите:

```json
{ "theme": "my-brand" }
```

6. Соберите / Publish из Rider как обычно.

`active.json` из исходников копируется в output.  
Опционально явно: `-p:LauncherActiveTheme=my-brand` (перезапишет active в output).

---

## Вариант 2: сборка в панели Gml

Панель скачивает исходники и компилирует на сервере. Бренд кладётся **один раз** в каталог данных:

```text
{InstallationDirectory}/LauncherBrand/
  active.json                 ← { "theme": "my-brand" }
  Themes/                     ← или сразу папки тем в корне LauncherBrand
    my-brand/
      theme.json
      logo.svg
    default/                  ← необязательно, если хотите переопределить стандарт
```

Альтернатива без подпапки `Themes/`:

```text
LauncherBrand/
  active.json
  my-brand/
    theme.json
    logo.svg
```

### Что делает панель

1. **Скачивание** исходников → подставляет host/folder → **копирует `LauncherBrand` → `src/Gml.Launcher/Themes`**
2. **Сборка** → перед compile снова применяет `LauncherBrand` (можно обновить шаблон без повторного download)

Игрок по-прежнему ничего не выбирает: в готовом exe уже ваш `active.json` и папка темы.

`InstallationDirectory` — каталог установки Gml на сервере (там же, где папка `Launcher/` с исходниками).

Шаблон можно залить через файловый менеджер панели / SSH / том Docker — главное путь `LauncherBrand`.

---

## Структура шаблона `theme.json`

| Раздел | Назначение |
|--------|------------|
| `Colors` | Палитра |
| `Visibility` | Скрыть/показать блоки UI |
| `Layout` | Сайдбар Left/Right, `BrandName` |
| `Assets` | Логотип |

### Visibility

`News`, `Online`, `ServerInfo`, `SocialLinks`, `ModsListButton`, `Logo`, `ProfileButton`, `LogoutButton`, `Sidebar`, `Footer`

### Layout

- `SidebarPosition`: `Left` \| `Right`
- `BrandName`: текст у логотипа

### Assets

`logo.svg` / `logo.png` в папке темы или `Assets.Logo`.

Пример: `Themes/samples/ocean` (только в репозитории, в сборку не копируется).

---

## Обновление ядра Gml

| Сценарий | Действие владельца |
|----------|-------------------|
| Rider | Обновили submodule/репо → ваша `Themes/my-brand` и `active.json` остаются |
| Панель | Скачали новую версию лаунчера → `LauncherBrand` на сервере подставится снова при download/build |

Не храните бренд в `Views/` — только в шаблоне.

---

## Чего система не делает

- Переключатель тем у игрока.
- Полностью свободная вёрстка из JSON без слотов в коде — только именованные слоты; новые появляются в обновлениях Gml.

---

## Для разработчиков

- Лаунчер: `ThemeService.ApplyActive()`, `Themes/` + `LauncherActiveTheme` в csproj
- Панель: `GitHubService.ApplyBrandTheme`, вызов из `GitHubLauncherHub` (Download + Compile)
- Каталог бренда на сервере: `{InstallationDirectory}/LauncherBrand`
