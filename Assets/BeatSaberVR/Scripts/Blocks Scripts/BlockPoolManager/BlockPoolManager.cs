using UnityEngine;

namespace BeatSaberVR
{
    public enum HalfType { Left, Right, Top, Bottom }

    public class BlockPoolManager : Singleton<BlockPoolManager>
    {
        [Header("Block prefabs")]
        public BlockBehavior redBlockPrefab;
        public BlockBehavior blueBlockPrefab;

        [Header("Wall")]
        public WallBehavior wallPrefab;

        [Header("Red block cut halves")]
        public BlockCutEffect redHalfLeftPrefab;
        public BlockCutEffect redHalfRightPrefab;
        public BlockCutEffect redHalfTopPrefab;
        public BlockCutEffect redHalfBottomPrefab;

        [Header("Blue block cut halves")]
        public BlockCutEffect blueHalfLeftPrefab;
        public BlockCutEffect blueHalfRightPrefab;
        public BlockCutEffect blueHalfTopPrefab;
        public BlockCutEffect blueHalfBottomPrefab;

        [Header("Cut particle prefabs")]
        public PooledParticle cutParticleRedPrefab;
        public PooledParticle cutParticleBluePrefab;

        [Header("Pool sizes")]
        public int blockPoolSize = 20;
        public int halfPoolSize = 30;  // per half type
        public int particlePoolSize = 15;
        public int wallPoolSize = 5;

        private ObjectPool<BlockBehavior> redPool, bluePool;
        private ObjectPool<WallBehavior> wallPool;
        private ObjectPool<BlockCutEffect> redLeftPool, redRightPool, redTopPool, redBottomPool;
        private ObjectPool<BlockCutEffect> blueLeftPool, blueRightPool, blueTopPool, blueBottomPool;
        private ObjectPool<PooledParticle> redParticlePool, blueParticlePool;

        public override void Awake()
        {
            base.Awake();

            Transform t = transform;

            redPool = new ObjectPool<BlockBehavior>(redBlockPrefab, blockPoolSize, t);
            bluePool = new ObjectPool<BlockBehavior>(blueBlockPrefab, blockPoolSize, t);

            wallPool = new ObjectPool<WallBehavior>(wallPrefab, wallPoolSize, t);

            // Red halves
            redLeftPool = new ObjectPool<BlockCutEffect>(redHalfLeftPrefab, halfPoolSize, t);
            redRightPool = new ObjectPool<BlockCutEffect>(redHalfRightPrefab, halfPoolSize, t);
            redTopPool = new ObjectPool<BlockCutEffect>(redHalfTopPrefab, halfPoolSize, t);
            redBottomPool = new ObjectPool<BlockCutEffect>(redHalfBottomPrefab, halfPoolSize, t);

            // Blue halves
            blueLeftPool = new ObjectPool<BlockCutEffect>(blueHalfLeftPrefab, halfPoolSize, t);
            blueRightPool = new ObjectPool<BlockCutEffect>(blueHalfRightPrefab, halfPoolSize, t);
            blueTopPool = new ObjectPool<BlockCutEffect>(blueHalfTopPrefab, halfPoolSize, t);
            blueBottomPool = new ObjectPool<BlockCutEffect>(blueHalfBottomPrefab, halfPoolSize, t);

            redParticlePool = new ObjectPool<PooledParticle>(cutParticleRedPrefab, particlePoolSize, t);
            blueParticlePool = new ObjectPool<PooledParticle>(cutParticleBluePrefab, particlePoolSize, t);
        }

        // ── Blocks ─────────────────────────────────────────────
        public BlockBehavior GetBlock(BlockColor color)
        {
            return (color == BlockColor.Red) ? redPool.Get() : bluePool.Get();
        }

        public void ReturnBlock(BlockBehavior block)
        {
            block.ResetState();
            block.gameObject.SetActive(false);
            if (block.blockColor == BlockColor.Red) redPool.ReturnToPool(block);
            else bluePool.ReturnToPool(block);
        }

        // ── Walls ──────────────────────────────────────────────
        public WallBehavior GetWall()
        {
            var wall = wallPool.Get();
            wall.gameObject.SetActive(true);
            return wall;
        }

        public void ReturnWall(WallBehavior wall)
        {
            wall.gameObject.SetActive(false);
            wallPool.ReturnToPool(wall);
        }

        // ── Cut halves ────────────────────────────
        public BlockCutEffect GetHalf(HalfType type, BlockColor color)
        {
            var pool = GetHalfPool(type, color);
            var half = pool.Get();
            half.gameObject.SetActive(true);
            return half;
        }

        public void ReturnHalf(BlockCutEffect half, HalfType type, BlockColor color)
        {
            half.gameObject.SetActive(false);
            GetHalfPool(type, color).ReturnToPool(half);
        }

        private ObjectPool<BlockCutEffect> GetHalfPool(HalfType type, BlockColor color)
        {
            if (color == BlockColor.Red)
            {
                return type switch
                {
                    HalfType.Left => redLeftPool,
                    HalfType.Right => redRightPool,
                    HalfType.Top => redTopPool,
                    _ => redBottomPool
                };
            }
            else
            {
                return type switch
                {
                    HalfType.Left => blueLeftPool,
                    HalfType.Right => blueRightPool,
                    HalfType.Top => blueTopPool,
                    _ => blueBottomPool
                };
            }
        }

        // ── Particles ──────────────────────────────────────────
        public void PlayCutParticle(Vector3 position, BlockColor color)
        {
            var pool = (color == BlockColor.Red) ? redParticlePool : blueParticlePool;
            var particle = pool.Get();
            if (particle == null) return;
            particle.Play(position, p => pool.ReturnToPool(p));
        }
    }
}