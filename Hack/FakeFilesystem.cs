using System.Collections.Frozen;

namespace mcarthey.com.Hack;

// Static fake filesystem for the puzzle. Everything a cracker sees on
// disk lives here. Content deliberately seeds the WarGames "Joshua"
// puzzle across several files so multiple exploration paths reveal
// the password.
public static class FakeFilesystem
{
    public sealed record DirEntry(string Name, bool IsDir, int Size = 0);

    public static readonly FrozenDictionary<string, List<DirEntry>> Directories = new Dictionary<string, List<DirEntry>>(StringComparer.OrdinalIgnoreCase)
    {
        ["/"] = new()
        {
            new("etc", true),
            new("home", true),
            new("usr", true),
            new("var", true),
            new("README", false, 128),
        },
        ["/etc"] = new()
        {
            new("motd", false, 187),
            new("passwd", false, 224),
            new("hostname", false, 16),
            new("resolv.conf", false, 42),
        },
        ["/home"] = new()
        {
            new("falken", true),
        },
        ["/home/falken"] = new()
        {
            new("photos", true),
            new("README.old", false, 96),
            new("notes.txt", false, 84),
        },
        ["/home/falken/photos"] = new()
        {
            new("family.jpg", false, 41823),
            new("dog.jpg", false, 18902),
            new("joshua_5th_birthday.jpg", false, 52104),
        },
        ["/usr"] = new()
        {
            new("bin", true),
        },
        ["/usr/bin"] = new()
        {
            new("cat", false, 43872),
            new("ls", false, 121440),
            new("history", false, 8712),
            new("login", false, 44528),
            new("logout", false, 8104),
            new("whoami", false, 7288),
        },
        ["/var"] = new()
        {
            new("log", true),
        },
        ["/var/log"] = new()
        {
            new("auth.log", false, 4096),
            new("syslog", false, 8192),
        },
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    // File contents. Keys are lowercased absolute paths.
    public static readonly FrozenDictionary<string, string> Files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["/readme"] = "Nothing to see here. Try HELP if you're lost.\n",
        ["/etc/motd"] = """
            * * * MCARTHEY-VAX-01 * * *

            This system runs old code.
            Backdoor is still active per S.F. request.
            Do not remove without operator approval.

            -- sysadmin, 1983-06-03
            """,
        ["/etc/passwd"] = """
            root:x:0:0:root:/root:/bin/sh
            daemon:x:2:2:daemon:/usr/sbin:/usr/sbin/nologin
            falken:x:1000:1000:Stephen Falken,,,:/home/falken:/bin/sh
            netop:x:11:11:Network Operator:/var/lib/netop:/usr/sbin/nologin
            """,
        ["/etc/hostname"] = "mcarthey-vax-01\n",
        ["/etc/resolv.conf"] = "nameserver 10.0.0.1\nsearch mcarthey.local\n",
        ["/home/falken/readme.old"] = """
            The system is old. So am I.
            Backdoor is still open. Kid's name still works.

              -- S.F.
            """,
        ["/home/falken/notes.txt"] = "Reminder: pick up Joshua from soccer at 4pm. He forgot his cleats again.\n",
        ["/var/log/auth.log"] = """
            Aug 14 21:03:14 mcarthey-vax-01 sshd[1834]: Failed password for root from 45.148.10.202 port 53102 ssh2
            Aug 14 21:04:22 mcarthey-vax-01 sshd[1835]: Failed password for admin from 121.4.99.181 port 47210 ssh2
            Aug 14 21:04:29 mcarthey-vax-01 sshd[1836]: Failed password for pi from 121.4.99.181 port 47228 ssh2
            Aug 14 21:07:44 mcarthey-vax-01 sshd[1841]: Invalid user oracle from 194.226.16.71
            Aug 14 21:08:03 mcarthey-vax-01 sshd[1842]: Failed password for postgres from 194.226.16.71 port 33221 ssh2
            """,
        ["/var/log/syslog"] = "(truncated. tail rotates every 24h.)\n",
        // Photos: binary, can't cat. Return themed error.
        ["/home/falken/photos/family.jpg"] = "cat: /home/falken/photos/family.jpg: binary file, cannot display\n",
        ["/home/falken/photos/dog.jpg"] = "cat: /home/falken/photos/dog.jpg: binary file, cannot display\n",
        ["/home/falken/photos/joshua_5th_birthday.jpg"] = "cat: /home/falken/photos/joshua_5th_birthday.jpg: binary file, cannot display\n",
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
