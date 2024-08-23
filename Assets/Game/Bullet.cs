using UnityEngine;
/// <summary>
/// používá se na střelách při samotné hře
/// </summary>
public class Bullet : MonoBehaviour
{
    public float direction = 1;
    public float pushForce;

    private float speed = Constants.BULLET_SPEED;

    private void Start()
    {
        GameObject o = Instantiate(GeneralGameManager.generalInstance.bulletParticle);
        o.transform.position = transform.position;

        if (direction == 1)
        {
            o.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            o.transform.eulerAngles = new Vector3(0, 0, 180);
        }
        
    }
    void Update()
    {
        transform.position += new Vector3(direction * speed, 0) * Time.deltaTime;

        if (Mathf.Abs(transform.position.x) > 100)
            Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
            collision.gameObject.GetComponent<Controler>().BulletPush(direction, pushForce);
        
        Destroy(gameObject);
    }
}
