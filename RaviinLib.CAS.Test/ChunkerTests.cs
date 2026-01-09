namespace RaviinLib.CAS.Tests
{
    public class ChunkerTests
    {
        readonly IChunkComparerStrict Comp = new IChunkComparerStrict();

        readonly Dictionary<string, IChunk> StringChunkPairs = new Dictionary<string, IChunk>()
        {
            #region BaseChunks
            { "x",new BaseChunk("x")},
            { "2^3",new BaseChunk(2,null,3)},
            { "2^(x+x)",new ChainChunk(1, new BaseChunk(2), new ChainChunk(1,new SumChunk(new List<IChunk>() { new BaseChunk("x"), new BaseChunk("x") }),new BaseChunk(1)))},
            { "(x+x)^2",new ChainChunk(1, new SumChunk(new List<IChunk>() { new BaseChunk("x"), new BaseChunk("x") }), new BaseChunk(2))},
            { "2",new BaseChunk(2)},
            { "2x",new BaseChunk(2, "x")},
            { "2x^2",new BaseChunk(2, "x",2)},
            { "2x^2x",new ChainChunk(1,new BaseChunk(2, "x"),new BaseChunk(2,"x")) },
        
            #endregion

            #region FuncChunk
            {"2Sin(2x^2)"       ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Sin,2)},
            {"2Cos(2x^2)"       ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Cos,2)},
            {"2Tan(2x^2)"       ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Tan,2)},
            {"2Sec(2x^2)"       ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Sec,2)},
            {"2Csc(2x^2)"       ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Csc,2)},
            {"2Cot(2x^2)"       ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Cot,2)},
            {"2Sinh(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Sinh,2)},
            {"2Cosh(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Cosh,2)},
            {"2Tanh(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Tanh,2)},
            {"2Sech(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Sech,2)},
            {"2Csch(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Csch,2)},
            {"2Coth(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Coth,2)},
            {"2ASin(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ASin,2)},
            {"2ACos(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ACos,2)},
            {"2ATan(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ATan,2)},
            {"2ASec(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ASec,2)},
            {"2ACsc(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ACsc,2)},
            {"2ACot(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ACot,2) },
            {"2ASinh(2x^2)"     ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ASinh,2)},
            {"2ACosh(2x^2)"     ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ACosh,2)},
            {"2ATanh(2x^2)"     ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ATanh,2)},
            {"2ASech(2x^2)"     ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ASech,2)},
            {"2ACsch(2x^2)"     ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ACsch,2)},
            {"2ACoth(2x^2)"     ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ACoth,2) },
            {"2Exp(2x^2)"       ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Exp,2) },
            {"2ln(2x^2)"        ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.ln,2) },
            {"2log(2x^2)"       ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.log,2) },
            {"2log(2x^2,2x^2)"  ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.log,2){SecondChunk= new BaseChunk(2,"x",2)} },
            {"2sqrt(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.sqrt,2) },
            {"2cbrt(2x^2)"      ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.cbrt,2) },
            {"2Abs(2x^2,2x^2)"  ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Abs,2){SecondChunk= new BaseChunk(2,"x",2)} },
            {"2Min(2x^2,2x^2)"  ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Min,2){SecondChunk= new BaseChunk(2,"x",2)} },
            {"2Max(2x^2,2x^2)"  ,       new FuncChunk(new BaseChunk(2,"x",2),Functions.Max,2){SecondChunk= new BaseChunk(2,"x",2)} },
            {"2Sin(2x^2)^2"     ,       new ChainChunk(2,new FuncChunk(new BaseChunk(2,"x",2),Functions.Sin),new BaseChunk(2))},
            {"xSin(x)", new ProductChunk(new BaseChunk("x"),new FuncChunk(new BaseChunk("x"),Functions.Sin)) },
            #endregion

            #region Variables
            {"x2"   ,new BaseChunk("x2")},
            {"x.2"  ,new BaseChunk("x.2")},
            {"xxx.2",new BaseChunk("xxx.2")},
            {"AbC.2",new BaseChunk("AbC.2")},
            {"😊"   ,new BaseChunk("😊")},
            #endregion

            //{"", },
            #region ProductChunk
            {"2*2"                              , new ProductChunk(new BaseChunk(2),new BaseChunk(2))},
            {"x*x"                              , new ProductChunk(new BaseChunk("x"),new BaseChunk("x"))},
            {"x^2*x^2"                          , new ProductChunk(new BaseChunk(1,"x",2),new BaseChunk(1,"x",2))},
            {"x^2*x+x"                          , new SumChunk(new List<IChunk>(){new ProductChunk(new BaseChunk(1,"x",2),new BaseChunk("x")),new BaseChunk("x") })},
            {"(x^2)*(x+x)/((x^2)*(x+x))/x*2/2"  ,
                new ProductChunk(
                    new ProductChunk(
                        new ProductChunk(
                            new ProductChunk(
                                new ProductChunk(
                                    new ChainChunk(
                                        1,
                                        new BaseChunk(1,"x",2),
                                        new BaseChunk(1)
                                    ),
                                    new ChainChunk(1,
                                        new SumChunk(new List<IChunk>(){ new BaseChunk("x"),new BaseChunk("x")}),
                                        new BaseChunk(1)
                                        )
                                    ),
                                new ChainChunk(
                                    1,
                                    new ChainChunk(
                                        1,
                                        new ProductChunk(
                                            new ChainChunk(
                                                1,
                                                new BaseChunk(1,"x",2),
                                                new BaseChunk(1)
                                            ),
                                            new ChainChunk(
                                                1,
                                                new SumChunk(new List<IChunk>(){ new BaseChunk("x"),new BaseChunk("x")}),
                                                new BaseChunk(1)
                                            )
                                        ),
                                        new BaseChunk(1)
                                    ),
                                    new BaseChunk(-1)
                                )
                            ),
                            new ChainChunk(
                                1,
                                new BaseChunk("x"),
                                new BaseChunk(-1)
                            )
                        ),
                        new BaseChunk(2)
                    ),
                    new ChainChunk(
                        1,
                        new BaseChunk(2),
                        new BaseChunk(-1)
                    )
                ) },
            #endregion

            #region ChainChunk
            {"(x)"              , new ChainChunk(1,new BaseChunk("x"),new BaseChunk(1))},
            {"2(x)"             , new ChainChunk(2,new BaseChunk("x"),new BaseChunk(1))},
            {"x(x)"             , new ProductChunk(new BaseChunk("x"),new ChainChunk(1,new BaseChunk("x"),new BaseChunk(1)))},
            {"2(x)^2"           , new ChainChunk(2,new BaseChunk("x"),new BaseChunk(2))},
            {"2(x)^x"           , new ChainChunk(2,new BaseChunk("x"),new BaseChunk("x"))},
            {"2(x)^x^x"         , new ChainChunk(2,new BaseChunk("x"),new ChainChunk(1,new BaseChunk("x"),new BaseChunk("x")))},
            {"2(x)^(x^x)"       , new ChainChunk(2,new BaseChunk("x"),new ChainChunk(1,new ChainChunk(1,new BaseChunk("x"),new BaseChunk("x")),new BaseChunk(1)))},
            {"2(x)^2(x^x)^2"    , new ChainChunk(2,new BaseChunk("x"),new ChainChunk(2,new ChainChunk(1,new BaseChunk("x"),new BaseChunk("x")),new BaseChunk(2)))},
            #endregion

            #region SumChunk
            {"2+2"                      , new SumChunk(new List<IChunk>(){new BaseChunk(2), new BaseChunk(2) })},
            {"2+x"                      , new SumChunk(new List<IChunk>(){new BaseChunk(2), new BaseChunk("x") })},
            {"x+x"                      , new SumChunk(new List<IChunk>(){new BaseChunk("x"), new BaseChunk("x") })},
            {"2-2"                      , new SumChunk(new List<IChunk>(){new BaseChunk(2), new BaseChunk(-2) })},
            {"2-x"                      , new SumChunk(new List<IChunk>(){new BaseChunk(2), new BaseChunk(-1,"x") })},
            {"x-x"                      , new SumChunk(new List<IChunk>(){new BaseChunk("x"), new BaseChunk(-1,"x") })},
            {"2+2+x+x-2-2-x-x+(x)-(x)"  , new SumChunk(new List<IChunk>(){new BaseChunk(2), new BaseChunk(2), new BaseChunk("x"), new BaseChunk("x"), new BaseChunk(-2) , new BaseChunk(-2), new BaseChunk(-1,"x"), new BaseChunk(-1,"x"), new ChainChunk(1,new BaseChunk("x"), new BaseChunk(1)), new ChainChunk(-1,new BaseChunk("x"), new BaseChunk(1)) })},
            {"2+-2", new SumChunk(new List<IChunk>(){new BaseChunk(2), new BaseChunk(-2) })},
            #endregion

            #region Scientific Notation
            {"2.x",  new BaseChunk(2,"x")},

            {"xE"   , new BaseChunk(1,"xE")   },
            {"1xE"  , new BaseChunk(1,"xE")   },
            {"1xE^2"  , new BaseChunk(1,"xE",2)   },
            {"1E+2x", new BaseChunk(100,"x") },
            {"1E-2x", new BaseChunk(0.01,"x")  },
            //{"1E-x" , new SumChunk(new List<IChunk>(){ new BaseChunk(1,"E"), new BaseChunk(-1,"x")})    }, // Should Throw
            //{"1E+x" , new SumChunk(new List<IChunk>(){ new BaseChunk(1,"E"), new BaseChunk(1,"x")})    },  // Should Throw
            #endregion

        };
        
        readonly List<string> FailureStringChunkPairs = new List<string>()
        {
            #region FuncChunk
            {"Sin(x)2"   },
            {"Sin(x)x"   },
            {"xSin(x)"   },
            #endregion

            #region Variables
            {".x"      },
            { "1E-x" },
            {"1E+x" },
            #endregion

            #region ChainChunk
            {"(x)2" },
            {"2(x)x"},
            {"2(x)x"},
            #endregion

            #region SumChunk
            {"2--2" },
            {"2++2" },
            {"2**2" },
            {"2//2" },
            #endregion
        };

        [SetUp]
        public void Setup()
        {
        }

        #region TestCases
        [TestCase("x")]
        [TestCase("2^3")]
        [TestCase("2^(x+x)")]
        [TestCase("(x+x)^2")]
        [TestCase("2")]
        [TestCase("2x")]
        [TestCase("2x^2")]
        [TestCase("2x^2x")]

        [TestCase("2Sin(2x^2)"     )]
        [TestCase("2Cos(2x^2)"     )]
        [TestCase("2Tan(2x^2)"     )]
        [TestCase("2Sec(2x^2)"     )]
        [TestCase("2Csc(2x^2)"     )]
        [TestCase("2Cot(2x^2)"     )]
        [TestCase("2Sinh(2x^2)"    )]
        [TestCase("2Cosh(2x^2)"    )]
        [TestCase("2Tanh(2x^2)"    )]
        [TestCase("2Sech(2x^2)"    )]
        [TestCase("2Csch(2x^2)"    )]
        [TestCase("2Coth(2x^2)"    )]
        [TestCase("2ASin(2x^2)"    )]
        [TestCase("2ACos(2x^2)"    )]
        [TestCase("2ATan(2x^2)"    )]
        [TestCase("2ASec(2x^2)"    )]
        [TestCase("2ACsc(2x^2)"    )]
        [TestCase("2ACot(2x^2)"    )]
        [TestCase("2ASinh(2x^2)"   )]
        [TestCase("2ACosh(2x^2)"   )]
        [TestCase("2ATanh(2x^2)"   )]
        [TestCase("2ASech(2x^2)"   )]
        [TestCase("2ACsch(2x^2)"   )]
        [TestCase("2ACoth(2x^2)"   )]
        [TestCase("2Exp(2x^2)"     )]
        [TestCase("2ln(2x^2)"      )]
        [TestCase("2log(2x^2)"     )]
        [TestCase("2log(2x^2,2x^2)")]
        [TestCase("2sqrt(2x^2)"    )]
        [TestCase("2cbrt(2x^2)"    )]
        [TestCase("2Abs(2x^2,2x^2)")]
        [TestCase("2Min(2x^2,2x^2)")]
        [TestCase("2Max(2x^2,2x^2)")]
        [TestCase("2Sin(2x^2)^2"   )]
        [TestCase("xSin(x)"        )]

        [TestCase("x2"             )]            
        [TestCase("x.2"            )]
        [TestCase("xxx.2"          )]
        [TestCase("AbC.2"          )]
        [TestCase("😊"             )]

        [TestCase("2*2"                            )]
        [TestCase("x*x"                            )]
        [TestCase("x^2*x^2"                        )]
        [TestCase("x^2*x+x"                        )]
        [TestCase("(x^2)*(x+x)/((x^2)*(x+x))/x*2/2")]

        [TestCase("(x)"              )]
        [TestCase("2(x)"             )]
        [TestCase("x(x)"             )]
        [TestCase("2(x)^2"           )]
        [TestCase("2(x)^x"           )]
        [TestCase("2(x)^x^x"         )]
        [TestCase("2(x)^(x^x)"       )]
        [TestCase("2(x)^2(x^x)^2"    )]

        [TestCase("2+2"                      )]
        [TestCase("2+x"                      )]
        [TestCase("x+x"                      )]
        [TestCase("2-2"                      )]
        [TestCase("2-x"                      )]
        [TestCase("x-x"                      )]
        [TestCase("2+2+x+x-2-2-x-x+(x)-(x)"  )]
        [TestCase("2+-2"                     )]

        [TestCase("2.x"     )]
        [TestCase("xE"      )]
        [TestCase("1xE"     )]
        [TestCase("1xE^2"   )]
        [TestCase("1E+2x"   )]
        [TestCase("1E-2x"   )]
        #endregion
        public void ChunkerChunkify(string Fx)
        {
            Assert.That(
                Chunker.Chunckify(Fx),
                Is.EqualTo(StringChunkPairs[Fx]).Using(Comp)
            );
        }


        [TestCase("Sin(x)2" )]
        [TestCase("Sin(x)x" )]

        [TestCase(".x"   )]
        [TestCase("1E")]
        [TestCase("1Ex")]
        [TestCase("1E-x")]
        [TestCase("1E+x" )]

        [TestCase("(x)2" )]
        [TestCase("2(x)x")]
        [TestCase("2(x)x")]

        [TestCase("2--2")]
        [TestCase("2++2" )]
        [TestCase("2**2" )]
        [TestCase("2//2" )]

        public void ChunkerChunkifyShouldThrow(string Fx)
        {
            Assert.Throws(typeof(Exception),() => Chunker.Chunckify(Fx));
        }
    }
}