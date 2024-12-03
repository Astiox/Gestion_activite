using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Gestion_activite;
using Gestion_activite.Gestion_activite;


namespace Gestion_activite
{
    internal class SingletonBDD
    {
        private MySqlConnection connection;
        private static SingletonBDD instance;
        public static int? TypeActiviteID { get; set; }

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
            try
            {
                using (var command = new MySqlCommand(query, GetConnection()))
                {
                    Console.WriteLine($"Query: {query}");

                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            if (!command.Parameters.Contains(param.Key))
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value);
                            }
                        }
                    }

                    command.ExecuteNonQuery();
                    Console.WriteLine("Query executed successfully.");
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"MySqlException: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }




        public static Dictionary<string, object> UtilisateurConnecte { get; set; }

        public static void Deconnecter()
        {
            SetUtilisateurConnecte(null); 
            Console.WriteLine("Déconnexion effectuée : UtilisateurConnecte est maintenant null.");
        }


        public MySqlDataReader ExecuteReader(string query, Dictionary<string, object> parameters = null)
{
    var connection = GetConnection();
    if (connection.State != System.Data.ConnectionState.Open)
    {
        connection.Open();
    }

    Console.WriteLine($"Executing query: {query}");
    if (parameters != null)
    {
        foreach (var param in parameters)
        {
            Console.WriteLine($"Parameter: {param.Key} = {param.Value}");
        }
    }

    var command = new MySqlCommand(query, connection);
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
            if (utilisateur != null)
            {
                App.Current.Resources["UtilisateurConnecte"] = utilisateur;

                Console.WriteLine($"Utilisateur connecté avec succès : {utilisateur["Nom"]} ({utilisateur["Role"]})");
            }
            else
            {
                if (App.Current.Resources.ContainsKey("UtilisateurConnecte"))
                {
                    App.Current.Resources.Remove("UtilisateurConnecte");
                }

                Console.WriteLine("Utilisateur déconnecté.");
            }
        }



        public static Dictionary<string, object> GetUtilisateurConnecte()
        {
            if (App.Current.Resources.ContainsKey("UtilisateurConnecte"))
            {
                var utilisateur = App.Current.Resources["UtilisateurConnecte"] as Dictionary<string, object>;
                Console.WriteLine($"Récupéré : {utilisateur["Nom"]} ({utilisateur["Role"]})");
                return utilisateur;
            }

            Console.WriteLine("Aucun utilisateur connecté actuellement.");
            return null;
        }




        public Dictionary<string, object> AuthentifierUtilisateur(string email, string motDePasse)
        {
            try
            {
                Console.WriteLine($"Debug: Recherche dans adherents pour Email: {email}");
                string queryAdherent = "SELECT ID, Nom, Prenom, 'Adherent' AS Role FROM adherents WHERE Email = @Email AND MotDePasse = @MotDePasse";
                using (var reader = ExecuteReader(queryAdherent, new Dictionary<string, object>
        {
            { "@Email", email },
            { "@MotDePasse", motDePasse }
        }))
                {
                    if (reader.Read())
                    {
                        Console.WriteLine("Debug: Utilisateur trouvé dans adherents.");
                        return new Dictionary<string, object>
                {
                    { "ID", reader["ID"].ToString() },
                    { "Nom", reader["Nom"].ToString() },
                    { "Prenom", reader["Prenom"].ToString() },
                    { "Email", email },
                    { "Role", "Adherent" }
                };
                    }
                }

                Console.WriteLine($"Debug: Recherche dans administrateurs pour Email: {email}");
                string queryAdmin = "SELECT ID, Nom, Prenom, 'Admin' AS Role FROM administrateurs WHERE Email = @Email AND MotDePasse = @MotDePasse";
                using (var reader = ExecuteReader(queryAdmin, new Dictionary<string, object>
        {
            { "@Email", email },
            { "@MotDePasse", motDePasse }
        }))
                {
                    if (reader.Read())
                    {
                        Console.WriteLine("Debug: Utilisateur trouvé dans administrateurs.");
                        return new Dictionary<string, object>
                {
                    { "ID", reader.GetInt32("ID") },
                    { "Nom", reader["Nom"].ToString() },
                    { "Prenom", reader["Prenom"].ToString() },
                    { "Email", email },
                    { "Role", "Admin" }
                };
                    }
                }

                Console.WriteLine("Debug: Aucun utilisateur trouvé.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur dans AuthentifierUtilisateur: {ex.Message}");
                throw;
            }
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
                Console.WriteLine($"Erreur EmailExiste: {ex.Message}");
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


        public void AjouterAdherent(string id, string nom, string prenom, DateTime dateNaissance, string adresse, string motDePasse, string email, DateTime dateInscription)
        {
            string query = "INSERT INTO adherents (Nom, Prenom, DateNaissance, Adresse, MotDePasse, Email, DateInscription) " +
                           "VALUES (@nom, @prenom, @dateNaissance, @adresse, @motDePasse, @email, @dateInscription)";
            ExecuteNonQuery(query, new Dictionary<string, object>
    {
        { "@nom", nom },
        { "@prenom", prenom },
        { "@dateNaissance", dateNaissance },
        { "@adresse", adresse },
        { "@motDePasse", motDePasse },
        { "@email", email },
        { "@dateInscription", dateInscription }
    });
        }





        public bool VerifierConnexion(string email, string motDePasse)
        {
            try
            {
                string query = @"
            SELECT COUNT(*) 
            FROM adherents 
            WHERE Email = @Email AND MotDePasse = @MotDePasse
            UNION ALL
            SELECT COUNT(*) 
            FROM administrateurs 
            WHERE Email = @Email AND MotDePasse = @MotDePasse";

                using (var command = new MySqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@MotDePasse", motDePasse);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    Console.WriteLine($"Debug: Nombre d'utilisateurs trouvés = {count}");

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur VerifierConnexion: {ex.Message}");
                return false;
            }
        }






        public void SupprimerAdherent(string id)
        {
            string query = "DELETE FROM adherents WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object> { { "@id", id } });
        }


        public void ModifierAdherent(string id, string nom, string prenom, DateTime dateNaissance, string adresse, DateTime dateInscription)
        {
            string query = "UPDATE adherents SET Nom = @nom, Prenom = @prenom, DateNaissance = @dateNaissance, Adresse = @adresse, DateInscription = @dateInscription WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@id", id },
                { "@nom", nom },
                { "@prenom", prenom },
                { "@dateNaissance", dateNaissance },
                { "@adresse", adresse },
                { "@dateInscription", dateInscription }
            });
        }







        public List<Dictionary<string, object>> ObtenirAdherents()
        {
            string query = "SELECT ID, Nom, Prenom, DateNaissance, Adresse, Email FROM adherents";
            var adherents = new List<Dictionary<string, object>>();

            using (var reader = ExecuteReader(query))
            {
                while (reader.Read())
                {
                    adherents.Add(new Dictionary<string, object>
                    {
                        { "ID", reader.IsDBNull(reader.GetOrdinal("ID")) ? null : reader["ID"].ToString() },
                        { "Nom", reader.IsDBNull(reader.GetOrdinal("Nom")) ? string.Empty : reader["Nom"].ToString() },
                        { "Prenom", reader.IsDBNull(reader.GetOrdinal("Prenom")) ? string.Empty : reader["Prenom"].ToString() },
                        { "DateNaissance", reader.IsDBNull(reader.GetOrdinal("DateNaissance")) ? null : reader.GetDateTime(reader.GetOrdinal("DateNaissance")) },
                        { "Adresse", reader.IsDBNull(reader.GetOrdinal("Adresse")) ? string.Empty : reader["Adresse"].ToString() },
                        { "Email", reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader["Email"].ToString() }
                    });
                }
            }

            return adherents;
        }


        public List<Activite> GetActivitesParType(int typeActiviteID)
        {
            var activites = new List<Activite>();
            string query = "SELECT * FROM activites WHERE TypeActiviteID = @TypeActiviteID";

            using (var command = new MySqlCommand(query, GetConnection()))
            {
                command.Parameters.AddWithValue("@TypeActiviteID", typeActiviteID);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        activites.Add(new Activite
                        {
                            ID = reader.GetInt32("ID"),
                            Nom = reader.GetString("Nom"),
                            Description = reader.GetString("Description"),
                            CoutOrganisation = reader.GetDecimal("CoutOrganisation"),
                            PrixVente = reader.GetDecimal("PrixVente"),
                            MoyenneNotes = reader.IsDBNull(reader.GetOrdinal("MoyenneNotes")) ? 0 : reader.GetDecimal("MoyenneNotes"),
                            NombreParticipants = reader.GetInt32("NombreParticipants"),
                            Image = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? "Assets/default.jpg" : reader.GetString("ImageUrl"),
                            TypeActiviteID = reader.GetInt32("TypeActiviteID")
                        });
                    }
                }
            }

            return activites;
        }






        public void AjouterActivite(string nom, int typeActiviteID, string description, decimal coutOrganisation, decimal prixVente, string imageUrl)
        {
            string query = "INSERT INTO activites (Nom, TypeActiviteID, Description, CoutOrganisation, PrixVente, ImageUrl) " +
                           "VALUES (@nom, @typeActiviteID, @description, @coutOrganisation, @prixVente, @imageUrl)";

            var parameters = new Dictionary<string, object>
    {
        { "@nom", nom },
        { "@typeActiviteID", typeActiviteID },
        { "@description", description },
        { "@coutOrganisation", coutOrganisation },
        { "@prixVente", prixVente },
        { "@imageUrl", imageUrl } 
    };

            ExecuteNonQuery(query, parameters);
        }




        public void SupprimerActivite(int id)
        {
            string query = "DELETE FROM activites WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object> { { "@id", id } });
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
            var seances = new List<Seance>();

            try
            {
                string query = "SELECT * FROM seances WHERE ActiviteID = @ActiviteID AND Date = @Date";
                using (var command = new MySqlCommand(query, GetConnection()))
                {
                    command.Parameters.AddWithValue("@ActiviteID", activiteID);
                    command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd")); 

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
                                PlacesRestantes = reader.GetInt32("PlacesRestantes")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des séances : {ex.Message}");
            }

            return seances;
        }

        public void ReserverPlace(int seanceID)
        {
            try
            {
                string query = "UPDATE seances SET PlacesRestantes = PlacesRestantes - 1 WHERE ID = @SeanceID";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SeanceID", seanceID);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la réservation de la place : {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }



        public void AjouterSeance(int activiteID, DateTime date, TimeSpan horaire, int placesTotales)
        {
            string query = "INSERT INTO seances (ActiviteID, Date, Horaire, PlacesRestantes, PlacesTotales) VALUES (@activiteID, @date, @horaire, @placesTotales, @placesTotales)";
            ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@activiteID", activiteID },
                { "@date", date },
                { "@horaire", horaire },
                { "@placesTotales", placesTotales }
            });
        }

        public void ModifierSeance(int id, DateTime date, TimeSpan horaire, int placesTotales)
        {
            string query = "UPDATE seances SET Date = @date, Horaire = @horaire, PlacesTotales = @placesTotales, PlacesRestantes = @placesTotales WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@id", id },
                { "@date", date },
                { "@horaire", horaire },
                { "@placesTotales", placesTotales }
            });
        }

        public void SupprimerSeance(int id)
        {
            string query = "DELETE FROM seances WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object> { { "@id", id } });
        }
        public List<Participation> ObtenirParticipations(string adherentID)
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
                        AdherentID = reader["AdherentID"].ToString(),
                        SeanceID = reader.GetInt32("SeanceID"),
                        Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetDecimal("Note")
                    });
                }
            }
            return participations;
        }
        public object ExecuteScalar(string query, Dictionary<string, object> parameters)
        {
            try
            {
                using (var command = new MySqlCommand(query, GetConnection()))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    return command.ExecuteScalar();
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"MySqlException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public bool ParticipationExiste(string adherentID, int activiteID)
        {
            bool existe = false;

            try
            {
                string query = "SELECT COUNT(*) FROM participations WHERE AdherentID = @AdherentID AND ActiviteID = @ActiviteID";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AdherentID", adherentID);
                    command.Parameters.AddWithValue("@ActiviteID", activiteID);

                    connection.Open();
                    existe = Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la vérification de la participation : {ex.Message}");
            }
            finally
            {
                connection.Close();
            }

            return existe;
        }



        public void AjouterParticipation(string adherentID, int seanceID, decimal? note = null)
        {
            try
            {
                string query = "INSERT INTO participations (AdherentID, SeanceID, Note) VALUES (@AdherentID, @SeanceID, @Note)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AdherentID", adherentID);
                    command.Parameters.AddWithValue("@SeanceID", seanceID);
                    command.Parameters.AddWithValue("@Note", note.HasValue ? (object)note.Value : DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout de la participation : {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }







        public ObservableCollection<Categorie> GetCategories()
        {
            var categories = new ObservableCollection<Categorie>();
            string query = "SELECT * FROM categories";

            using (var reader = ExecuteReader(query))
            {
                while (reader.Read())
                {
                    categories.Add(new Categorie
                    {
                        ID = reader.GetInt32("ID"),
                        NomCategorie = reader.GetString("Nom"),
                        Description = reader.GetString("Description")
                    });
                }
            }
            return categories;
        }

        public void AjouterCategorie(string nom, string description)
        {
            string query = "INSERT INTO categories (Nom, Description) VALUES (@nom, @description)";
            ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@nom", nom },
                { "@description", description }
            });
        }

        public void ModifierCategorie(int id, string nom, string description)
        {
            string query = "UPDATE categories SET Nom = @nom, Description = @description WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@id", id },
                { "@nom", nom },
                { "@description", description }
            });
        }

        public void SupprimerCategorie(int id)
        {
            string query = "DELETE FROM categories WHERE ID = @id";
            ExecuteNonQuery(query, new Dictionary<string, object> { { "@id", id } });
        }
        public List<TypeActivite> GetTypesActivites()
        {
            var typesActivites = new List<TypeActivite>();
            string query = "SELECT ID, Nom, Description, Image FROM typeactivite";

            try
            {
                if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                {
                    connection.Open();
                }

                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            typesActivites.Add(new TypeActivite
                            {
                                ID = reader.GetInt32("ID"),
                                Nom = reader.GetString("Nom"),
                                Description = reader.GetString("Description"),
                                Image = reader.GetString("Image")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des types d'activités : {ex.Message}");
                throw;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return typesActivites;
        }

        public void AjouterTypeActivite(TypeActivite typeActivite)
        {
            string query = "INSERT INTO typeactivite (Nom, Description, Image) VALUES (@Nom, @Description, @Image)";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Nom", typeActivite.Nom);
                command.Parameters.AddWithValue("@Description", typeActivite.Description);
                command.Parameters.AddWithValue("@Image", typeActivite.Image);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void ModifierTypeActivite(TypeActivite typeActivite)
        {
            string query = "UPDATE typeactivite SET Nom = @Nom, Description = @Description, Image = @Image WHERE ID = @ID";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ID", typeActivite.ID);
                command.Parameters.AddWithValue("@Nom", typeActivite.Nom);
                command.Parameters.AddWithValue("@Description", typeActivite.Description);
                command.Parameters.AddWithValue("@Image", typeActivite.Image);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void SupprimerTypeActivite(int id)
        {
            string updateQuery = "UPDATE activites SET TypeActiviteID = NULL WHERE TypeActiviteID = @ID";
            string deleteQuery = "DELETE FROM typeactivite WHERE ID = @ID";

            using (var command = new MySqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@ID", id);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }

            using (var command = new MySqlCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@ID", id);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }



        public int GetTotalAdherents()
        {
            string query = "SELECT COUNT(*) FROM adherents";
            return ExecuteScalar(query);
        }

        public int GetTotalActivites()
        {
            string query = "SELECT COUNT(*) FROM activites";
            return ExecuteScalar(query);
        }

        public Dictionary<string, int> GetAdherentsParActivite()
        {
            string query = @"SELECT activites.Nom AS ActiviteNom, COUNT(participations.ID) AS NombreAdherents
                            FROM participations
                            INNER JOIN seances ON participations.SeanceID = seances.ID
                            INNER JOIN activites ON seances.ActiviteID = activites.ID
                            GROUP BY activites.Nom";

            var result = new Dictionary<string, int>();
            using (var reader = ExecuteReader(query))
            {
                while (reader.Read())
                {
                    result[reader.GetString("ActiviteNom")] = reader.GetInt32("NombreAdherents");
                }
            }
            return result;
        }

        public Dictionary<string, decimal> GetMoyenneNotesParActivite()
        {
            string query = @"
        SELECT activites.Nom AS ActiviteNom, AVG(participations.Note) AS MoyenneNotes
        FROM participations
        INNER JOIN seances ON participations.SeanceID = seances.ID
        INNER JOIN activites ON seances.ActiviteID = activites.ID
        WHERE participations.Note IS NOT NULL
        GROUP BY activites.Nom";

            var result = new Dictionary<string, decimal>();
            using (var reader = ExecuteReader(query))
            {
                while (reader.Read())
                {
                    result[reader.GetString("ActiviteNom")] = reader.IsDBNull(reader.GetOrdinal("MoyenneNotes")) ? 0 : reader.GetDecimal("MoyenneNotes");
                }
            }
            return result;
        }

        private int ExecuteScalar(string query)
        {
            using (var command = new MySqlCommand(query, GetConnection()))
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public List<Dictionary<string, object>> ObtenirActivites()
        {
            string query = "SELECT ID, Nom, Description FROM activites"; 
            var activites = new List<Dictionary<string, object>>();

            using (var reader = ExecuteReader(query))
            {
                while (reader.Read())
                {
                    activites.Add(new Dictionary<string, object>
            {
                { "ID", reader["ID"].ToString() },
                { "Nom", reader["Nom"].ToString() },
                { "Description", reader["Description"].ToString() }
            });
                }
            }

            return activites;
        }


        public List<Dictionary<string, object>> Obteniradherents()
        {
            string query = "SELECT ID, Nom, Prenom, DateNaissance, Adresse, Email FROM adherents";
            var adherents = new List<Dictionary<string, object>>();

            using (var reader = ExecuteReader(query))
            {
                while (reader.Read())
                {
                    adherents.Add(new Dictionary<string, object>
            {
                { "ID", reader["ID"].ToString() },
                { "Nom", reader["Nom"].ToString() },
                { "Prenom", reader["Prenom"].ToString() },
                { "DateNaissance", reader["DateNaissance"].ToString() },
                { "Adresse", reader["Adresse"].ToString() },
                { "Email", reader["Email"].ToString() }
            });
                }
            }

            return adherents;
        }
        public bool ActiviteExiste(string nom)
        {
            string query = "SELECT COUNT(*) FROM activites WHERE Nom = @nom";
            var parameters = new Dictionary<string, object>
    {
        { "@nom", nom }
    };
            return Convert.ToInt32(ExecuteScalar(query, parameters)) > 0;
        }

    }
}
 
