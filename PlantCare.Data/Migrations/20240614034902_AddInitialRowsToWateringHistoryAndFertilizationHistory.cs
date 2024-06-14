using Microsoft.EntityFrameworkCore.Migrations;
using PlantCare.Data.DbModels;

#nullable disable

namespace PlantCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialRowsToWateringHistoryAndFertilizationHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check and insert initial rows for WateringHistory table
            migrationBuilder.Sql(@$"

                INSERT INTO {nameof(WateringHistory)} (Id, PlantId, CareTime)
                SELECT
                    lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-' || '4' || substr(hex(randomblob(2)),2) || '-' || substr('AB89', 1 + (abs(random()) % 4) , 1) || substr(hex(randomblob(2)),2) || '-' || hex(randomblob(6))),
                    p.Id,
                    p.LastWatered
                FROM Plants p
                WHERE NOT EXISTS (SELECT 1 FROM {nameof(WateringHistory)} WHERE PlantId = p.Id);

            ");

            // Check and insert initial rows for FertilizationHistory table
            migrationBuilder.Sql(@$"

                INSERT INTO {nameof(FertilizationHistory)} (Id, PlantId, CareTime)
                SELECT
                    lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-' || '4' || substr(hex(randomblob(2)),2) || '-' || substr('AB89', 1 + (abs(random()) % 4) , 1) || substr(hex(randomblob(2)),2) || '-' || hex(randomblob(6))),
                    p.Id,
                    p.LastFertilized
                FROM Plants p
                WHERE NOT EXISTS (SELECT 1 FROM {nameof(FertilizationHistory)} WHERE PlantId = p.Id);
            ");

            ////migrationBuilder.Sql(@$"

            ////    INSERT INTO {nameof(WateringHistory)} (Id, PlantId, CareTime)
            ////    SELECT NEWID(), p.Id, p.LastWatered
            ////    FROM Plants p;
            ////    WHERE NOT EXISTS (SELECT 1 FROM {nameof(WateringHistory)});

            ////");

            //// Check and insert initial rows for FertilizationHistory table
            //migrationBuilder.Sql(@$"

            //    INSERT INTO {nameof(FertilizationHistory)} (Id, PlantId, CareTime)
            //    SELECT NEWID(), p.Id, p.LastFertilized
            //    FROM Plants p;
            //    WHERE NOT EXISTS (SELECT 1 FROM {nameof(FertilizationHistory)});
            //");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback logic to remove the initial rows if the migration is rolled back
            migrationBuilder.Sql(@$"
                DELETE FROM {nameof(WateringHistory)}
                WHERE PlantId IN (SELECT Id FROM Plants)
                AND CareTime IN (SELECT LastWatered FROM Plants);
                ");

            migrationBuilder.Sql(@$"
                DELETE FROM {nameof(FertilizationHistory)}
                WHERE PlantId IN (SELECT Id FROM Plants)
                AND CareTime IN (SELECT LastFertilized FROM Plants);
                ");
        }
    }
}