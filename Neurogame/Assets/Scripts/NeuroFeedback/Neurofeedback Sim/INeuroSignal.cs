public interface INeuroSignal
{
    float Alpha { get; }
    float Beta { get; }
    float Theta { get; }
    float Quality { get; } // 0..1
}
