using UnityEngine;
using System.Diagnostics;

/// <summary>
/// třída kterou mají připojenou všichni ostatní hráči ve hře po síti
/// vypočítává interpolaci svojí pozice
/// </summary>
public class Enemy : MonoBehaviour
{
    public string playerName;
    // list všech následujících pozic tohoto hráče které client přijmul ze sítě
    private LinearList<TimePositionPair> positions;
    
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Vector2 lastFramePos;
    private float direction;
    void Update()
    {
        long milisec = Stopwatch.Frequency / 1_000;
        long timeBetveenServerTicks = 1_000 / Constants.TICK_RATE * milisec;
        long extraDelay = milisec * 2;

        // čas o který je pozice daného hráče spožděna
        // musí být spožděn alepoň o jednu dobu mezi packetama aby mohla fungovat interpolace
        // je spožděna o dvě pokud by se jeden packet stratil (je použit UDP protokol)
        long delay = extraDelay + 2 * timeBetveenServerTicks;

        positions = NetworkGameManager.positions.Get(playerName);
        
        // pokud nemáme ani 2 pozice není mezi čím interpolovat
        if (positions.Count < 2)
            return;

        long currentTime = NetworkGameManager.instance.watch.ElapsedTicks - delay;

        // odstranění pozic z listu které už neplatí protože jsou moc staré
        while (positions[1] != null)
        {
            if (positions.Count < 3)
                break;

            if (positions[1].time < currentTime)
                positions.RemoveFirst();
            else
                break;
        }

        // žádná interpolace pokud je první pozice ještě v budoucnosti
        if (positions[0].time > currentTime)
            return;

        transform.position = CalcCurrentPosition(positions[0], positions[1], currentTime);

        //Vector2 velocity = lastFramePos - (Vector2)transform.position;
        //animator.SetFloat("hSpeed", Mathf.Abs(velocity.x));
        //lastFramePos = transform.position;*/

        spriteRenderer.flipX = positions[0].direction;
    }
    Vector2 CalcCurrentPosition(TimePositionPair a, TimePositionPair b, long currentTime)
    {
        long duration = b.time - a.time;
        long currentTimeDifference = currentTime - a.time;
        // výpočet kde v čase se právě nacházíme mezi pozicí A a B v rosahu 0..1 
        float currentTimeFraction = currentTimeDifference / (float)duration;

        return Vector2.LerpUnclamped(a.position, b.position, currentTimeFraction);
    }
}
public class TimePositionPair
{
    public Vector2 position;
    public long time;
    public bool direction;

    public TimePositionPair(Vector2 position, long client_tick, bool direction)
    {
        this.position = position;
        this.time = client_tick;
        this.direction = direction;
    }
}
