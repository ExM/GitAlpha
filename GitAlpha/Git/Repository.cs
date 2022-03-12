using System.IO;
using LibGit2Sharp;

namespace GitAlpha.Git;

public class Repository
{
    private readonly DirectoryInfo _root;

    public Repository(DirectoryInfo root)
    {
        _root = root;
    }

    public IReadOnlyList<Revision> GetRevisions()
    {
        var result = new List<Revision>();
        using var repo = new LibGit2Sharp.Repository(_root.FullName);
        foreach (var commit in repo.Commits)
        {
            result.Add(new Revision()
            {
                Id = commit.Id.ToString(),
                Author = commit.Author.Name,
                Subject = commit.MessageShort,
                CommitDate = commit.Committer.When.UtcDateTime
            });
        }

        return result;
    }
}