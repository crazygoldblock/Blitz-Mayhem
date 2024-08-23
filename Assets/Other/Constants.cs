/// <summary>
/// všechny konstanty které se používají po celě aplikaci
/// </summary>
public class Constants
{
    public const int TICK_RATE = 20;

    public const int MAX_PLAYERS_COUNT = 4;
    public const int MIN_PLAYERS_COUNT = 2;
    
    public const int BROADCAST_CODE = 170;
    public const int SERVER_CLOSE_CODE = 180;
    public const int GAME_LOADED_CODE = 190;
    public const int START_GAME_CODE = 200;

    // id packetů
    public const int POSITION_ID = 1;
    public const int BULLET_ID = 2;
    public const int SERVER_INFO_ID = 3;
    public const int JOIN_ID = 4;
    public const int PING_ID = 5; 
    public const int LEAVE_ID = 6;
    public const int START_GAME_ID = 7;
    public const int DEATH_ID = 8;
    public const int UPGRADE_SPAWN_ID = 9;
    public const int UPGRADE_PICKUP_ID = 10;
    
    public const int CHAT_ID = 12;
    public const int PLAYER_NAMES_ID = 13;
    public const int ROUND_RESET_ID = 14;

    // multicast adresa kde clienti můžou hledat servery
    public const string DISCOVERY_ADDR = "239.0.0.1";
    // multicast adresa kde clienti komunikují při samotné hře
    public const string MLUTICAST_ADDR = "239.0.0.";
    public const int ADDR_START_NUMBER = 2;
    public const int MLUTICAST_PORT = 5002;


    public const float PLAYER_SPEED = 50f;
    public const float PLAYER_JUMP = 500f;
    public const float BULLET_PUSH = 200f;
    public const float FIRE_RATE = 0.4f;

    public const float PLAYER_SPEED_UP = PLAYER_SPEED * 1.5f;
    public const float PLAYER_JUMP_UP = PLAYER_JUMP * 2f;
    public const float BULLET_PUSH_UP = BULLET_PUSH * 1.2f;
    public const float FIRE_RATE_UP = FIRE_RATE / 2f;

    public const float UPGRADE_DURATION = 15f;
    public const float TIME_BETWEEN_UPGRADES = 10f;

    public const float BULLET_SPEED = 15f;

}
