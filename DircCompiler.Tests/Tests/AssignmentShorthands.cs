namespace DircCompiler.Tests;

public class AssignmentShorthands
{
    [Fact]
    public void Addition()
    {
        string source =
        """
        int x = 2;
        x += 3;
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 2 r0 _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 3 r0
        mov fp _ r1
        store r0 r1 _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void Subtraction()
    {
        string source =
        """
        int x = 2;
        x -= 3;
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 2 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 r1 3 r0
        mov fp _ r1
        store r0 r1 _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void BitwiseAnd()
    {
        string source =
        """
        int x = 2;
        x &= 3;
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 2 r0 _
        mov fp _ r0
        load r0 _ r1
        and|i2 r1 3 r0
        mov fp _ r1
        store r0 r1 _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void BitwiseOr()
    {
        string source =
        """
        int x = 2;
        x |= 0b01000000;
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 2 r0 _
        mov fp _ r0
        load r0 _ r1
        or|i2 r1 0b01000000 r0
        mov fp _ r1
        store r0 r1 _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void BitwiseXor()
    {
        string source =
        """
        int x = 2;
        x ^= 0b00000111;
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 2 r0 _
        mov fp _ r0
        load r0 _ r1
        xor|i2 r1 0b00000111 r0
        mov fp _ r1
        store r0 r1 _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void PlusPlus()
    {
        string source =
        """
        int x = 2;
        x++;
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 2 r0 _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 1 r0
        mov fp _ r1
        store r0 r1 _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void MinusMinus()
    {
        string source =
        """
        int x = 2;
        x--;
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 2 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 r1 1 r0
        mov fp _ r1
        store r0 r1 _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
