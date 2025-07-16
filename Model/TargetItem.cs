using ZeroTrace.Enums;

namespace ZeroTrace.Model;

public sealed record TargetItem(string Path, TargetType Type, string Size);