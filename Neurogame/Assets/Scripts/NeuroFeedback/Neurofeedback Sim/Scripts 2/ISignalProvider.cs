public interface ISignalProvider
{
    /// Returns a normalized signal in [0..1]
    float GetSignal01();
}