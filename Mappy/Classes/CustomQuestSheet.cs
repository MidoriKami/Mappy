using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Classes;

[Sheet( "Quest" )]
public class CustomQuestSheet : Quest {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public LazyRow<Level> [,] ToDoLocation { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        
    public override void PopulateData( RowParser parser, GameData gameData, Language language ) {
        base.PopulateData( parser, gameData, language );

        ToDoLocation = new LazyRow< Level >[ 24, 8 ];
            
        for (var i = 0; i < 24; i++) {
            for (var j = 0; j < 8; j++) {
                ToDoLocation[i, j] = new LazyRow< Level >(gameData, parser.ReadColumn< uint >( 1221 + (j * 24) + i ), language);
            }
        }
    }
}