using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Origins
{
    [Serializable]
    class WaterFlow
    {
        public static List<WaterFlow> Flows = new List<WaterFlow>();

        private int gridX;
        private int gridY;
        private Timer spreadTmr = new Timer();

        public WaterFlow(int gridX, int gridY)
        {
            if (Tile.Map[gridX, gridY].Type != Tile.TileType.Dirt || !Tile.PointIsInMap(gridX, gridY))
                return;

            GameObject obj = GameObject.Objects.Find(x => x.GridX == gridX && x.GridY == gridY);
            obj?.Destroy();
            Flows.Add(this);

            this.gridX = gridX;
            this.gridY = gridY;
            Tile.ChangeTile(gridX, gridY, Tile.TileType.Water);
            spreadTmr.Interval = 200;
            spreadTmr.Start();
            spreadTmr.Tick += SpreadTmr_Tick;
        }

        [OnDeserializing]
        protected void OnDeserializing(StreamingContext context)
        {
            spreadTmr.Interval = 200;
            spreadTmr.Start();
            spreadTmr.Tick += SpreadTmr_Tick;
        }

        public void Destroy()
        {
            spreadTmr.Dispose();
            Flows.Remove(this);
        }

        private void SpreadTmr_Tick(object sender, EventArgs e)
        {
            if (Tile.PointIsInMap(gridX - 1, gridY) && Tile.Map[gridX - 1, gridY].Type == Tile.TileType.Dirt)
                new WaterFlow(gridX - 1, gridY);
            if (Tile.PointIsInMap(gridX + 1, gridY) && Tile.Map[gridX + 1, gridY].Type == Tile.TileType.Dirt)
                new WaterFlow(gridX + 1, gridY);
            if (Tile.PointIsInMap(gridX, gridY - 1) && Tile.Map[gridX, gridY - 1].Type == Tile.TileType.Dirt)
                new WaterFlow(gridX, gridY - 1);
            if (Tile.PointIsInMap(gridX, gridY + 1) && Tile.Map[gridX, gridY + 1].Type == Tile.TileType.Dirt)
                new WaterFlow(gridX, gridY + 1);

            Destroy();
        }
    }
}
