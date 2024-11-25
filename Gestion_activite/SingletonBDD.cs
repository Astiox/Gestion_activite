using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace Gestion_activite
{
    internal class SingletonBDD
    {
        private MySqlConnection connection;
        private static SingletonBDD instance;

        private SingletonBDD()
        {
            string connectionString = "Server=cours.cegep3r.info;Database=h2024_420417ri_gr2-eq2;Uid=2020960;Pwd=Kujv4090/;";
            connection = new MySqlConnection(connectionString);
        }

        public static SingletonBDD GetInstance()
        {
            if (instance == null)
                instance = new SingletonBDD();
            return instance;
        }

        public MySqlConnection GetConnection()
        {
            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                connection.Open();
            }
            return connection;
        }


        public void ExecuteNonQuery(string query, Dictionary<string, object> parameters)
        {
            using (var command = new MySqlCommand(query, GetConnection()))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                        command.Parameters.AddWithValue(param.Key, param.Value);
                }
                command.ExecuteNonQuery();
            }
        }
        public static Dictionary<string, object> UtilisateurConnecte { get; set; }

        public static void Deconnecter()
        {
            UtilisateurConnecte = null;
        }

        public MySqlDataReader ExecuteReader(string query, Dictionary<string, object> parameters = null)
        {
            var command = new MySqlCommand(query, GetConnection());
            if (parameters != null)
            {
                foreach (var param in parameters)
                    command.Parameters.AddWithValue(param.Key, param.Value);
            }
            return command.ExecuteReader();
        }

        public void CloseConnection()
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();
        }
        public static void SetUtilisateurConnecte(Dictionary<string, object> utilisateur)
        {
            App.Current.Resources["UtilisateurConnecte"] = utilisateur;
        }

        public static Dictionary<string, object> GetUtilisateurConnecte()
        {
            if (App.Current.Resources.ContainsKey("UtilisateurConnecte"))
            {
                return App.Current.Resources["UtilisateurConnecte"] as Dictionary<string, object>;
            }
            return null;
        }

        public Dictionary<string, object> AuthentifierUtilisateur(string email, string motDePasse)
        {
            string query = "SELECT ID, Nom, Prenom FROM adherents WHERE Email = @Email AND MotDePasse = @MotDePasse";
            using (var reader = ExecuteReader(query, new Dictionary<string, object>
    {
        { "@Email", email },
        { "@MotDePasse", motDePasse }
    }))
            {
                if (reader.Read())
                {
                    int id;
                    if (!int.TryParse(reader["ID"].ToString(), out id))
                    {
                        throw new FormatException("La valeur de l'ID utilisateur n'est pas au bon format.");
                    }

                    return new Dictionary<string, object>
            {
                { "ID", id },
                { "Nom", reader["Nom"].ToString() },
                { "Prenom", reader["Prenom"].ToString() },
                { "Email", email }
            };
                }
            }

            return null; 
        }


        public bool EmailExiste(string email)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM adherents WHERE Email = @Email";
                using (var command = new MySqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la vérification de l'email : {ex.Message}");
                return false;
            }
        }
        public ObservableCollection<DateTime> getDateSeance()
        {
            ObservableCollection<DateTime> resultats = new ObservableCollection<DateTime>();

            try
            {
                string query = "SELECT DISTINCT Date FROM seances";
                using (var command = new MySqlCommand(query, GetConnection()))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultats.Add(reader.GetDateTime("Date")); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return resultats;
        }


        public void AjouterAdherent(string id, string nom, string prenom, DateTime dateNaissance, string adresse, string motDePasse, string email)
        {
            string query = "INSERT INTO adherents (Nom, Prenom, DateNaissance, Adresse, MotDePasse, Email) VALUES (@nom, @prenom, @dateNaissance, @adresse, @motDePasse, @email)";
            ExecuteNonQuery(query, new Dictionary<string, object>
    {
        { "@nom", nom },
        { "@prenom", prenom },
        { "@dateNaissance", dateNaissance },
        { "@adresse", adresse },
        { "@motDePasse", motDePasse },
        { "@email", email }
    });
        }





        public bool VerifierConnexion(string email, string motDePasse)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM adherents WHERE Email = @Email AND motDePasse = @motDePasse";
                using (var command = new MySqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@motDePasse", motDePasse);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la vérification de la connexion : {ex.Message}");
                return false;
            }
        }



        public void ModifierAdherent(string id, string nom, string prenom, DateTime dateNaissance, string adresse)
        {
            string query = "UPDATE adherents SET Nom = @nom, Prenom = @prenom, DateNaissance = @dateNaissance, Adresse = @adresse WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@id", id },
                { "@nom", nom },
                { "@prenom", prenom },
                { "@dateNaissance", dateNaissance },
                { "@adresse", adresse }
            });
        }

        public void SupprimerAdherent(string id)
        {
            string query = "DELETE FROM adherents WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object> { { "@id", id } });
        }

        public List<Dictionary<string, object>> Obteniradherents()
        {
            string query = "SELECT * FROM adherents";
            var liste = new List<Dictionary<string, object>>();
            using (var reader = ExecuteReader(query))
            {
                while (reader.Read())
                {
                    var adherent = new Dictionary<string, object>
                    {
                        { "ID", reader["ID"] },
                        { "Nom", reader["Nom"] },
                        { "Prenom", reader["Prenom"] },
                        { "DateNaissance", reader["DateNaissance"] },
                        { "Adresse", reader["Adresse"] },
                        { "DateInscription", reader["DateInscription"] }
                    };
                    liste.Add(adherent);
                }
            }
            return liste;
        }
        public ObservableCollection<Activite> GetActivites()
        {
            var activites = new ObservableCollection<Activite>();
            try
            {
                string query = "SELECT ID, Nom, CategorieID, Description, CoutOrganisation, PrixVente, MoyenneNotes, NombreParticipants, ImageUrl FROM activites";
                using (var command = new MySqlCommand(query, GetConnection()))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var activite = new Activite
                            {
                                ID = reader.GetInt32("ID"),
                                Nom = reader.GetString("Nom"),
                                CategorieID = reader.GetInt32("CategorieID"),
                                Description = reader.GetString("Description"),
                                CoutOrganisation = reader.GetDecimal("CoutOrganisation"),
                                PrixVente = reader.GetDecimal("PrixVente"),
                                MoyenneNotes = reader.IsDBNull(reader.GetOrdinal("MoyenneNotes")) ? 0 : reader.GetDecimal("MoyenneNotes"),
                                NombreParticipants = reader.GetInt32("NombreParticipants"),
                                Image = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? "Assets/default.jpg" : reader.GetString("ImageUrl")
                            };
                            activites.Add(activite);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des activités : {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return activites;
        }




        public void AjouterActivite(string nom, int categorieID, string description, decimal coutOrganisation, decimal prixVente)
        {
            string query = "INSERT INTO activites (Nom, CategorieID, Description, CoutOrganisation, PrixVente) VALUES (@nom, @categorieID, @description, @coutOrganisation, @prixVente)";
            ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@nom", nom },
                { "@categorieID", categorieID },
                { "@description", description },
                { "@coutOrganisation", coutOrganisation },
                { "@prixVente", prixVente }
            });
        }

        public void ModifierActivite(int id, string nom, int categorieID, string description, decimal coutOrganisation, decimal prixVente)
        {
            string query = "UPDATE activites SET Nom = @nom, CategorieID = @categorieID, Description = @description, CoutOrganisation = @coutOrganisation, PrixVente = @prixVente WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@id", id },
                { "@nom", nom },
                { "@categorieID", categorieID },
                { "@description", description },
                { "@coutOrganisation", coutOrganisation },
                { "@prixVente", prixVente }
            });
        }

        public void SupprimerActivite(int id)
        {
            string query = "DELETE FROM activites WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object> { { "@id", id } });
        }

        public List<Dictionary<string, object>> Obteniractivites()
        {
            string query = "SELECT * FROM activites";
            var liste = new List<Dictionary<string, object>>();
            using (var reader = ExecuteReader(query))
            {
                while (reader.Read())
                {
                    var activite = new Dictionary<string, object>
                    {
                        { "ID", reader["ID"] },
                        { "Nom", reader["Nom"] },
                        { "CategorieID", reader["CategorieID"] },
                        { "Description", reader["Description"] },
                        { "CoutOrganisation", reader["CoutOrganisation"] },
                        { "PrixVente", reader["PrixVente"] },
                        { "MoyenneNotes", reader["MoyenneNotes"] },
                        { "NombreParticipants", reader["NombreParticipants"] }
                    };
                    liste.Add(activite);
                }
            }
            return liste;
        }
        public List<DateTime> GetAvailableDates(int activiteID)
        {
            List<DateTime> dates = new List<DateTime>();
            string query = "SELECT DISTINCT Date FROM seances WHERE ActiviteID = @activiteID";

            using (var command = new MySqlCommand(query, GetConnection()))
            {
                command.Parameters.AddWithValue("@activiteID", activiteID);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dates.Add(reader.GetDateTime("Date"));
                    }
                }
            }
            return dates;
        }

        public List<Seance> GetSeances(int activiteID, DateTime date)
        {
            List<Seance> seances = new List<Seance>();
            string query = "SELECT ID, ActiviteID, Date, Horaire, PlacesDisponibles, PlacesTotales FROM seances WHERE ActiviteID = @activiteID AND Date = @date";

            using (var command = new MySqlCommand(query, GetConnection()))
            {
                command.Parameters.AddWithValue("@activiteID", activiteID);
                command.Parameters.AddWithValue("@date", date);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        seances.Add(new Seance
                        {
                            ID = reader.GetInt32("ID"),
                            ActiviteID = reader.GetInt32("ActiviteID"),
                            Date = reader.GetDateTime("Date"),
                            Horaire = reader.GetTimeSpan("Horaire"),
                            PlacesRestantes = reader.GetInt32("PlacesDisponibles"),
                            PlacesTotales = reader.GetInt32("PlacesTotales")
                        });
                    }
                }
            }
            return seances;
        }



        public void ReserverPlace(int seanceID)
        {
            string query = "UPDATE seances SET PlacesDisponibles = PlacesDisponibles - 1 WHERE ID = @seanceID AND PlacesDisponibles > 0";
            using (var command = new MySqlCommand(query, GetConnection()))
            {
                command.Parameters.AddWithValue("@seanceID", seanceID);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new Exception("Impossible de réserver une place. La séance est peut-être complète.");
                }
            }
        }


        public void AjouterSeance(int activiteID, DateTime dateSeance, int placesDisponibles)
            {
                string query = "INSERT INTO seances (ActiviteID, DateSeance, PlacesDisponibles, PlacesRestantes) VALUES (@activiteID, @dateSeance, @placesDisponibles, @placesDisponibles)";
                ExecuteNonQuery(query, new Dictionary<string, object>
    {
        { "@activiteID", activiteID },
        { "@dateSeance", dateSeance },
        { "@placesDisponibles", placesDisponibles }
    });
            }


            public void SupprimerSeance(int id)
            {
                string query = "DELETE FROM seances WHERE ID = @id";
                ExecuteNonQuery(query, new Dictionary<string, object> { { "@id", id } });
            }
        public List<Participation> ObtenirParticipations(int adherentID)
        {
            string query = "SELECT ID, AdherentID, SeanceID, Note FROM participations WHERE AdherentID = @AdherentID";

            List<Participation> participations = new List<Participation>();

            using (var reader = ExecuteReader(query, new Dictionary<string, object> { { "@AdherentID", adherentID } }))
            {
                while (reader.Read())
                {
                    participations.Add(new Participation
                    {
                        ID = reader.GetInt32("ID"),
                        AdherentID = reader.GetInt32("AdherentID"),
                        SeanceID = reader.GetInt32("SeanceID"),
                        Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetDecimal("Note")
                    });
                }
            }
            return participations;
        }
        private void ChargerParticipations()
        {
            if (SingletonBDD.UtilisateurConnecte != null)
            {
                int adherentID = (int)SingletonBDD.UtilisateurConnecte["ID"];
                var participations = SingletonBDD.GetInstance().ObtenirParticipations(adherentID);

                foreach (var participation in participations)
                {
                    Console.WriteLine($"Participation ID: {participation.ID}, Seance ID: {participation.SeanceID}, Note: {participation.Note}");
                }
            }
            else
            {
                Console.WriteLine("Aucun utilisateur connecté.");
            }
        }

        public void AjouterParticipation(int adherentID, int seanceID, decimal? note)
        {
            string query = "INSERT INTO participations (AdherentID, SeanceID, Note) VALUES (@AdherentID, @SeanceID, @Note)";

            ExecuteNonQuery(query, new Dictionary<string, object>
    {
        { "@AdherentID", adherentID },
        { "@SeanceID", seanceID },
        { "@Note", (object)note ?? DBNull.Value }
    });
        }




        public void SupprimerParticipation(int id)
            {
                string query = "DELETE FROM participations WHERE ID = @id";
                ExecuteNonQuery(query, new Dictionary<string, object> { { "@id", id } });
            }
        }
    }
