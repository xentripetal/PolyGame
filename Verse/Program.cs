try
{
    using var verse = new Verse();
    verse.Run();
}
catch (Exception e)
{
    Console.Error.Write(e.ToString());
}
