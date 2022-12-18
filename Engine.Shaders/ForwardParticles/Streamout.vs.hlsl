
struct VSParticle
{
    float3 position : POSITION;
    float3 velocity : VELOCITY;
    float4 random : RANDOM;
    float maxAge : MAX_AGE;

    uint type : TYPE;
    float emissionTime : EMISSION_TIME;
};

VSParticle main(VSParticle input)
{
    return input;
}
