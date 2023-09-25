struct [raypayload] HitInfo
{
    float3 color : read(caller, anyhit, closesthit) : write(caller, anyhit, closesthit, miss);
    float alpha : read(caller, anyhit) : write(caller, anyhit, closesthit, miss);
    float3 normal : read(caller, anyhit) : write(caller, anyhit, closesthit, miss);
    float distance : read(caller,anyhit) : write(caller, anyhit, closesthit, miss);
};

struct [raypayload] ShadowHitInfo
{
    bool isHit : read(caller) : write(caller, closesthit, miss);
};
