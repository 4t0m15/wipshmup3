using System;
using BenchmarkDotNet.Attributes;

[Microsoft.VSDiagnostics.CPUUsageDiagnoser]
[Microsoft.VSDiagnostics.DotNetObjectAllocDiagnoser]
public class PlayerAnimationBench
{
    private NaiveController _naive;
    private OptimizedController _opt;
    private string[] _sequence;

    [GlobalSetup]
    public void Setup()
    {
        _naive = new NaiveController();
        _opt = new OptimizedController();
        // Typical sequence: mostly same value with occasional switches
        _sequence = new[] { "right", "right", "right", "right", "left", "left", "up_down", "up_down", "right", "right" };
    }

    [Benchmark]
    public void NaiveSwitchLoop()
    {
        for (int r = 0; r < 10_000; r++)
        {
            foreach (var anim in _sequence)
            {
                _naive.SetAnimation(anim, play: true);
            }
        }
    }

    [Benchmark]
    public void OptimizedSwitchLoop()
    {
        for (int r = 0; r < 10_000; r++)
        {
            foreach (var anim in _sequence)
            {
                _opt.SetAnimation(anim, play: true);
            }
        }
    }

    private sealed class MockSprite
    {
        public string Animation { get; set; } = string.Empty;
        public bool IsPlaying { get; private set; }
        public void Play() => IsPlaying = true;
        public void Stop() => IsPlaying = false;
    }

    private sealed class NaiveController
    {
        private readonly MockSprite _sprite = new MockSprite();
        public void SetAnimation(string animName, bool play)
        {
            _sprite.Animation = animName; // always assigns
            if (play) _sprite.Play(); else _sprite.Stop();
        }
    }

    private sealed class OptimizedController
    {
        private readonly MockSprite _sprite = new MockSprite();
        private string _currentAnim = string.Empty;
        private bool _isPlaying;
        public void SetAnimation(string animName, bool play)
        {
            if (!ReferenceEquals(_currentAnim, animName) && _currentAnim != animName)
            {
                _sprite.Animation = animName;
                _currentAnim = animName;
            }
            if (play)
            {
                if (!_isPlaying) { _sprite.Play(); _isPlaying = true; }
            }
            else if (_isPlaying) { _sprite.Stop(); _isPlaying = false; }
        }
    }
}