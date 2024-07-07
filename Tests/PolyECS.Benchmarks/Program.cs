global using BenchmarkDotNet.Attributes;
global using BenchmarkDotNet.Running;


BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
