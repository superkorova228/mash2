namespace mash2.Core
{
    /// <summary>
    /// Все возможные состояния игры
    /// Enum - это список именованных констант
    /// </summary>
    public enum GameState
    {
        Boot,           // Инициализация (сцена Boot)
        MainMenu,       // Главное меню
        Settings,       // Меню настроек
        Gameplay,       // Основной геймплей
        Paused,         // Игра на паузе
        GameOver,       // Игрок проиграл
        Credits,        // Титры
        Loading         // Идёт загрузка сцены
    }
}