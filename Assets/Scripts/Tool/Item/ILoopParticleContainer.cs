using UnityEngine;

public interface ILoopParticleContainer
{
    bool UpdateLoopParticle(int passiveId, GameObject particle);
    void ResetLoopParticle();
}
