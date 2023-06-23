// ReSharper disable All

using Lumina.Text;
using Lumina.Data;
using Lumina.Data.Structs.Excel;
#pragma warning disable CS8618

namespace Lumina.Excel.GeneratedSheets
{
    [Sheet( "Quest" )]
    public partial class CustomQuestSheet : Quest
    {
        public LazyRow<Level> [,] ToDoLocation { get; set; }
        
        public override void PopulateData( RowParser parser, GameData gameData, Language language )
        {
            base.PopulateData( parser, gameData, language );

            ToDoLocation = new LazyRow< Level >[ 24, 8 ];
            
            for (var i = 0; i < 24; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    ToDoLocation[i, j] = new LazyRow< Level >(gameData, parser.ReadColumn< uint >( 1221 + (j * 24) + i ), language);
                }
            }
        }
    }
}