public class ProjectileSine : Projectile
{
    public float frequency = 5f;
    public float magnitude = 2f;
    private float birthTime;

    void Start() { birthTime = Time.time; }

    public override void Move()
    {
        // Move forward normally
        Vector3 tempPos = pos;
        tempPos.y += speed * Time.deltaTime;
        
        // Add the sine wave to the X axis
        tempPos.x += Mathf.Sin((Time.time - birthTime) * frequency) * magnitude;
        
        pos = tempPos;
    }
}