using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace UserStoriesApp
{
    public class Program
    {
        private const string USERNAME = "Yashashree-Salunke";
        private const string PASSWORD = "yash@1994";
        public static void Main(string[] args)
        {
           // Userstories userstories = new Userstories();
           // AcceptanceCriterias criteria = new AcceptanceCriterias();
            //create clone
            var co = new CloneOptions();
            co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = USERNAME, Password = PASSWORD };
            Repository.Clone("https://github.com/Yashashree-Salunke/UserStoriesFeature.git", "C:/Users/admin/Documents/GitHub/UserStoriesFeature");

            using (var repo = new Repository(@"C:/Users/admin/Documents/GitHub/UserStoriesFeature"))
            {
                var db = new ReqSpecContextContext();
                DirectoryInfo ParentDirectory = new DirectoryInfo(repo.Info.WorkingDirectory);
                foreach (var u in db.Userstories)
                {
                    if (u.RootId == null && u.ParentId == null)
                    {
                        if (u.HasChildren == true)
                        {

                            var FolderName = u.Title.Replace(" ", "_");
                            var Folder = Directory.CreateDirectory(
                                 Path.Combine(ParentDirectory.FullName, FolderName));

                            string title = u.Title.Replace(" ", "");
                            string fullpath = Path.Combine(repo.Info.WorkingDirectory, title + ".Feature");
                            var tag = (from t in db.Tags
                                       join us in db.UserstoryTags on t.Id equals us.TagId
                                       where us.UserstoryId == u.Id
                                       select t.Title).SingleOrDefault ();
                            var scenario = (from ac in db.AcceptanceCriterias
                                            join us in db.Userstories on ac.UserstoryId equals us.Id
                                            where ac.UserstoryId == u.Id
                                            select ac.Title).SingleOrDefault();
                            var GWT = (from ac in db.AcceptanceCriterias
                                       join us in db.Userstories on ac.UserstoryId equals us.Id
                                       where ac.UserstoryId == u.Id
                                       select ac.Gwt).SingleOrDefault();

                            var contents = "Feature:" + u.Title + "\n" + u.Description +
                                "\n" + "@" + tag + "\n" + "Scenario:" + scenario + "\n" + "\t" + GWT;

                            File.WriteAllText(fullpath, contents);
                            repo.Index.Add(title + ".Feature");
                            Commands.Stage(repo, fullpath);
                        }
                        else
                        {
                            string title = u.Title.Replace(" ", "");
                            string fullpath = Path.Combine(ParentDirectory.FullName, title + ".Feature");
                            var tag = (from t in db.Tags
                                       join us in db.UserstoryTags on t.Id equals us.TagId
                                       where us.UserstoryId == u.Id
                                       select t.Title).SingleOrDefault();
                            var scenario = (from ac in db.AcceptanceCriterias
                                            join us in db.Userstories on ac.UserstoryId equals us.Id
                                            where ac.UserstoryId == u.Id
                                            select ac.Title).SingleOrDefault();
                            var GWT = (from ac in db.AcceptanceCriterias
                                       join us in db.Userstories on ac.UserstoryId equals us.Id
                                       where ac.UserstoryId == u.Id
                                       select ac.Gwt).SingleOrDefault();

                            var content = "Feature:" + u.Title +"\n" + u.Description +
                                "\n" + "@" + tag + "\n" + "Scenario:" + scenario + "\n" + "\t" + GWT;

                            File.WriteAllText(fullpath, content);
                            repo.Index.Add(title + ".Feature");
                            Commands.Stage(repo, fullpath);
                           
                        }
                    }
                    if (u.ParentId != null)
                    {
                        if (u.HasChildren == true)
                        {
                            CreateFolder();
                        }
                        else
                        {
                            string Filename = u.Title.Replace(" ", "");
                            var title = (from s in db.Userstories where s.Id == u.ParentId select s.Title).Single();
                            List<string> folders = ParentDirectory.GetDirectories(title.Replace(" ", "_"), SearchOption.AllDirectories).Select(p => p.FullName).ToList();
                            string Fullpath = folders.Single();
                            string newpath = Path.Combine(Fullpath, Filename + ".Feature");
                            int fileIndex = repo.Info.WorkingDirectory.Count();
                            string file = newpath.Remove(0, fileIndex);
                            var tag = (from t in db.Tags
                                       join us in db.UserstoryTags on t.Id equals us.TagId
                                       where us.UserstoryId == u.Id
                                       select t.Title).SingleOrDefault();
                            var scenario = (from ac in db.AcceptanceCriterias
                                            join us in db.Userstories on ac.UserstoryId equals us.Id
                                            where ac.UserstoryId == u.Id
                                            select ac.Title).SingleOrDefault();
                            var GWT = (from ac in db.AcceptanceCriterias
                                       join us in db.Userstories on ac.UserstoryId equals us.Id
                                       where ac.UserstoryId == u.Id
                                       select ac.Gwt).SingleOrDefault();

                            var content = "Feature:" + u.Title + "\n" + u.Description +
                                "\n" + "@" + tag + "\n" + "Scenario:" + scenario + "\n" + "\t" + GWT;
                            File.WriteAllText(newpath, content);
                            repo.Index.Add(file);
                            Commands.Stage(repo, newpath);
                        }
                    }
                    void CreateFolder()
                    {
                        List<Userstories> list = new List<Userstories>();

                        string connectionString = "Server=(localdb)\\mssqllocaldb;Database=ReqSpecContext;Trusted_Connection=True;MultipleActiveResultSets=true";
                        SqlConnection con = new SqlConnection(connectionString);
                        SqlCommand command = new SqlCommand("SELECT Id,HasChildren,ParentId,Title FROM UserStories");
                        command.Connection = con;
                        command.Connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            Userstories us = new Userstories();
                            us.Id = (int)reader["Id"];
                            us.HasChildren = (bool)reader["HasChildren"];

                            if (reader["ParentId"] == DBNull.Value)
                            {
                                us.ParentId = 0;
                            }
                            else
                            {
                                us.ParentId = (int)reader["ParentId"];
                            }
                            us.Title = (string)reader["Title"];
                            list.Add(us);

                        }

                        foreach (var item in list)
                        {
                            if (item.ParentId == u.ParentId)
                            {
                                var FolderName = item.Title.Replace(" ", "_");
                                var title = list.Where(x => x.Id == item.ParentId).Select(p => p.Title).Single();
                                List<string> folders = ParentDirectory.GetDirectories(title.Replace(" ", "_"), SearchOption.AllDirectories).Select(p => p.FullName).ToList();
                                string Fullpath = folders.Single();
                                var folder = Directory.CreateDirectory(Path.Combine(Fullpath, FolderName));

                                string file = u.Title.Replace(" ", "");
                                string filepath = Path.Combine(Fullpath, title + ".Feature");
                                var tag = (from t in db.Tags
                                           join us in db.UserstoryTags on t.Id equals us.TagId
                                           where us.UserstoryId == u.Id
                                           select t.Title).Single();
                                var scenario = (from ac in db.AcceptanceCriterias
                                                join us in db.Userstories on ac.UserstoryId equals us.Id
                                                where ac.UserstoryId == u.Id
                                                select ac.Title).Single();
                                var GWT = (from ac in db.AcceptanceCriterias
                                           join us in db.Userstories on ac.UserstoryId equals us.Id
                                           where ac.UserstoryId == u.Id
                                           select ac.Gwt).Single();

                                var content = "Feature:" + u.Title + "\n" + u.Description +
                                    "\n" + "@" + tag + "\n" + "Scenario:" + scenario + "\n" + "\t" + GWT;

                                File.WriteAllText(filepath, content);
                                repo.Index.Add(title + ".Feature");
                                Commands.Stage(repo, filepath);


                            }
                        }
                    }
                }
                PushAndCommit();
                void PushAndCommit()
                {
                    Signature author = new Signature("Yashashree-Salunke", USERNAME, DateTime.Now);
                    Signature committer = author;
                    Commit commit = repo.Commit("Feature is added", author, committer);  // Commit to the repository

                    PushOptions options = new PushOptions();
                    options.CredentialsProvider = new CredentialsHandler(
                        (url, usernameFromUrl, types) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = USERNAME,
                                Password = PASSWORD
                            });
                    repo.Network.Push(repo.Branches["master"], options);
                }

            }
        }
    }
}
