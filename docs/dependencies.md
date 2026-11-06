# Зависимости проекта

Диаграмма исходной версии: она показывает, что нужно разобрать при внедрении VContainer.

## До: версия из ветки `main`

DI-фреймворка в проекте нет. Объекты связаны ссылками в инспекторе (их расставляет `GameBuilder`) и ручными вызовами `Init`.

```mermaid
classDiagram
    direction LR
    class GameController {
        <<MonoBehaviour>>
        +State
        +Score
        +WorldSpeed
        +Difficulty
        +HeldSource
    }
    class Spawner {
        <<MonoBehaviour>>
        +Begin()
        +Stop()
        +SpawnMeteor()
        +SpawnStar()
    }
    class Comet {
        <<MonoBehaviour>>
        +Holding
        +WorldSpeed
        +Launch()
    }
    class GameHud { <<MonoBehaviour>> }
    class Scroller {
        <<MonoBehaviour>>
        +Init(game, edge)
    }
    class Star {
        <<MonoBehaviour>>
        +Init(sparks)
    }
    class Meteor {
        <<MonoBehaviour>>
        +Launch()
    }
    class SparkField { <<MonoBehaviour>> }
    class Hazard { <<MonoBehaviour>> }

    GameController --> Comet : поле в инспекторе
    GameController --> Spawner : поле в инспекторе
    Spawner --> GameController : поле в инспекторе, цикл
    Spawner --> Comet : высота кометы
    Spawner --> SparkField : поле в инспекторе
    Spawner ..> Meteor : Instantiate
    Spawner ..> Star : Instantiate
    Spawner ..> Scroller : Init
    Scroller --> GameController : скорость мира
    Star --> SparkField : Init
    GameHud --> GameController : поле в инспекторе
    Comet ..> Star : столкновение
    Comet ..> Hazard : столкновение
```

Что здесь мешает:

| Проблема | Где | Чем плохо |
|---|---|---|
| Цикл зависимостей | `GameController` ⇄ `Spawner` | Ни один класс нельзя понять и проверить без другого |
| Класс делает всё сразу | `GameController`: ввод, состояние, счёт, скорость мира, заморозка времени | Нельзя заменить или проверить что-то одно |
| Скрытые статические зависимости | `Input` и `Time.timeScale` в `GameController`, `Camera.main` в `Spawner` | Их не видно в сигнатуре класса и нельзя подменить |
| Ручная передача зависимостей | `Scroller.Init`, `Star.Init` вызывает `Spawner` | `Spawner` знает, что нужно чужим классам |
| Логика живёт только в `MonoBehaviour` | Все классы | Проверить правила можно только на загруженной сцене |
| Лазейка для тестов | `GameController.HeldSource` | Код игры открыт ради тестов |
| Настройки разбросаны | Числа в полях `GameController`, `Comet`, `Spawner` | Баланс игры нельзя посмотреть и поменять в одном месте |
