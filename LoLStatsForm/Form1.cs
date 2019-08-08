using RiotNet;
using RiotNet.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;


namespace LoLStatsForm
{
    public partial class Form1 : Form
    {
        static Match T1;
        static IRiotClient cli;
        static Champion Champ;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int week = Int32.Parse(Week_Box.Text);

                string league;

                if (GP_RB.Checked)
                {
                    league = "GP";
                }
                else if (IBSG_RB.Checked)
                {
                    league = "IBSG";
                }
                else
                {
                    league = "FAIL";
                }

                long matchid = Int64.Parse(MatchID_Box.Text);
                TestAsync(matchid).Wait();

                Z_Team TeamA = new Z_Team(100, "Blue", "Temp");
                TeamA.Player_List.Add(new Z_Player("Temp", 1));
                TeamA.Player_List.Add(new Z_Player("Temp", 2));
                TeamA.Player_List.Add(new Z_Player("Temp", 3));
                TeamA.Player_List.Add(new Z_Player("Temp", 4));
                TeamA.Player_List.Add(new Z_Player("Temp", 5));


                Z_Team TeamB = new Z_Team(200, "Red", "Temp");
                TeamB.Player_List.Add(new Z_Player("Temp", 1));
                TeamB.Player_List.Add(new Z_Player("Temp", 2));
                TeamB.Player_List.Add(new Z_Player("Temp", 3));
                TeamB.Player_List.Add(new Z_Player("Temp", 4));
                TeamB.Player_List.Add(new Z_Player("Temp", 5));


                int i = 0;
                int b = 0;
                int r = 0;

                foreach (var item in T1.ParticipantIdentities)
                {
                    if (i <= 4)
                    {
                        TeamA.Player_List[b].Kills = T1.Participants[i].Stats.Kills;
                        TeamA.Player_List[b].Deaths = T1.Participants[i].Stats.Deaths;
                        TeamA.Player_List[b].Assists = T1.Participants[i].Stats.Assists;
                        TeamA.Player_List[b].Creeps = T1.Participants[i].Stats.TotalMinionsKilled + T1.Participants[i].Stats.NeutralMinionsKilled;
                        TeamA.Player_List[b].VisionScore = (int)T1.Participants[i].Stats.VisionScore;
                        TeamA.Player_List[b].Win = T1.Participants[i].Stats.Win;
                        TeamA.Player_List[b].GameTime = ((T1.GameDuration.Minutes * 60) + T1.GameDuration.Seconds);
                        TeamA.Player_List[b].Champion = T1.Participants[i].ChampionId;
                        TeamA.Player_List[b].CSD10 = (int)(T1.Participants[i].Timeline.CsDiffPerMinDeltas.ZeroToTen * 10);
                        b++;
                    }
                    else if (i > 4)
                    {
                        TeamB.Player_List[r].Kills = T1.Participants[i].Stats.Kills;
                        TeamB.Player_List[r].Deaths = T1.Participants[i].Stats.Deaths;
                        TeamB.Player_List[r].Assists = T1.Participants[i].Stats.Assists;
                        TeamB.Player_List[r].Creeps = T1.Participants[i].Stats.TotalMinionsKilled + T1.Participants[i].Stats.NeutralMinionsKilled;
                        TeamB.Player_List[r].VisionScore = (int)T1.Participants[i].Stats.VisionScore;
                        TeamB.Player_List[r].Win = T1.Participants[i].Stats.Win;
                        TeamB.Player_List[r].GameTime = ((T1.GameDuration.Minutes * 60) + T1.GameDuration.Seconds);
                        TeamB.Player_List[r].Champion = T1.Participants[i].ChampionId;
                        TeamB.Player_List[r].CSD10 = (int)(T1.Participants[i].Timeline.CsDiffPerMinDeltas.ZeroToTen * 10);
                        r++;
                    }
                    i++;
                }


                if (TeamA.Player_List[0].Win)
                {
                    TeamA.Player_List[0].SummonerName = BlueTop_Box.Text;
                    TeamA.Player_List[1].SummonerName = BlueJng_Box.Text;
                    TeamA.Player_List[2].SummonerName = BlueMid_Box.Text;
                    TeamA.Player_List[3].SummonerName = BlueBot_Box.Text;
                    TeamA.Player_List[4].SummonerName = BlueSup_Box.Text;
                    TeamA.TeamName = BlueTeam_Box.Text;

                    TeamB.Player_List[0].SummonerName = RedTop_Box.Text;
                    TeamB.Player_List[1].SummonerName = RedJng_Box.Text;
                    TeamB.Player_List[2].SummonerName = RedMid_Box.Text;
                    TeamB.Player_List[3].SummonerName = RedBot_Box.Text;
                    TeamB.Player_List[4].SummonerName = RedSup_Box.Text;
                    TeamB.TeamName = RedTeam_Box.Text;
                }
                else
                {
                    TeamB.Player_List[0].SummonerName = BlueTop_Box.Text;
                    TeamB.Player_List[1].SummonerName = BlueJng_Box.Text;
                    TeamB.Player_List[2].SummonerName = BlueMid_Box.Text;
                    TeamB.Player_List[3].SummonerName = BlueBot_Box.Text;
                    TeamB.Player_List[4].SummonerName = BlueSup_Box.Text;
                    TeamB.TeamName = BlueTeam_Box.Text;

                    TeamA.Player_List[0].SummonerName = RedTop_Box.Text;
                    TeamA.Player_List[1].SummonerName = RedJng_Box.Text;
                    TeamA.Player_List[2].SummonerName = RedMid_Box.Text;
                    TeamA.Player_List[3].SummonerName = RedBot_Box.Text;
                    TeamA.Player_List[4].SummonerName = RedSup_Box.Text;
                    TeamA.TeamName = RedTeam_Box.Text;
                }



                MySqlConnection conn;
                string connString = String.Format("server={0};user id={1}; password={2}; database={3};", "localhost", "USERNAME", "PASSWORD", "DATABASE"); //Change USERNAME, PASSWORD and DATABASE to your local MySql Instance information
                conn = new MySqlConnection(connString);
                conn.Open();

                IDbCommand command = conn.CreateCommand();
                command.CommandText = "USE DuoStats;";
                command.ExecuteNonQuery();

                foreach (var player in TeamB.Player_List)
                {
                    command.CommandText = "INSERT INTO " + league + "_PlayerStats (SummonerName, Kills, Deaths, Assists, Creeps, Lane, TeamName, VisionScore, Win, Week, GameTime, Champion, MatchID, Side) Values (\"" + player.SummonerName + "\", " + player.Kills + ", " + player.Deaths + ", " + player.Assists + ", " + player.Creeps + ", " + "\"" + player.Lane + "\", \"" + TeamB.TeamName + "\", " + player.VisionScore + ", " + player.Win + ", " + week + ", " + player.GameTime + ", " + player.Champion + ", " + matchid + ", \"" + TeamB.TeamSide + "\");";
                    command.ExecuteNonQuery();
                }

                foreach (var player in TeamA.Player_List)
                {
                    command.CommandText = "INSERT INTO " + league + "_PlayerStats (SummonerName, Kills, Deaths, Assists, Creeps, Lane, TeamName, VisionScore, Win, Week, GameTime, Champion, MatchID, Side) Values (\"" + player.SummonerName + "\", " + player.Kills + ", " + player.Deaths + ", " + player.Assists + ", " + player.Creeps + ", " + "\"" + player.Lane + "\", \"" + TeamA.TeamName + "\", " + player.VisionScore + ", " + player.Win + ", " + week + ", " + player.GameTime + ", " + player.Champion + ", " + matchid + ", \"" + TeamA.TeamSide + "\");";
                    command.ExecuteNonQuery();
                }


                conn.Close();




                MessageBox.Show("Completed!");
            }
            catch(Exception E)
            {
                MessageBox.Show("Failed.\n Reason: " + E.Message);
            }
        }


        static async Task TestAsync(long MatchID)
        {
            IRiotClient client = new RiotClient(new RiotClientSettings
            {
                ApiKey = "############" // Replace this with your API key, of course.
            });

            cli = client;

            Match match = await client.GetMatchAsync(MatchID, PlatformId.NA1).ConfigureAwait(false);
            T1 = match;
        }

        static async Task GetChampion(long ChampionID, int i)
        {
            Champion ch = await cli.GetChampionByIdAsync(T1.Participants[i].ChampionId, PlatformId.NA1).ConfigureAwait(false);
            Champ = ch;
        }

    }

    public class Z_Team
    {
        public int TeamId;
        public string TeamSide;
        public string TeamName;
        public List<Z_Player> Player_List;

        public Z_Team(int team_id, string team_side, string team_name)
        {
            TeamId = team_id;
            TeamSide = team_side;
            TeamName = team_name;
            Player_List = new List<Z_Player>();
        }

        public void AddPlayerToTeam(Z_Player player)
        {
            Player_List.Add(player);
        }

    }

    public class Z_Player
    {
        public string SummonerName;
        public int Kills;
        public int Deaths;
        public int Assists;
        public int Creeps;
        public int Lane;
        public int Champion;
        public int VisionScore;
        public int GameTime;
        public bool Win;
        public int CSD10;

        public Z_Player(string name, int k, int d, int a, int c, int l, int champ, int v, bool w)
        {
            SummonerName = name;
            Kills = k;
            Deaths = d;
            Assists = a;
            Creeps = c;
            Lane = l;
            Champion = champ;
            VisionScore = v;
            Win = w;
        }

        public Z_Player() { }

        public Z_Player(string name, int l)
        {
            SummonerName = name;
            Lane = l;
        }

        public void Print_Player()
        {
            Console.WriteLine(Lane + " " + SummonerName + " : " + Kills + "/" + Deaths + "/" + Assists);
        }
    }
}
