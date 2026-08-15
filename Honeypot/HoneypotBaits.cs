using System.Collections.Frozen;

namespace mcarthey.com.Honeypot;

// Bait catalog -- paths that bots probe and the fake responses we serve.
// Adding a new bait: append to _entries below. Aliases (e.g. /pma → phpmyadmin)
// point at the same BaitResponse instance so log analytics see one bait per family.
public static class HoneypotBaits
{
    public sealed record BaitResponse(string Name, string ContentType, string Body, int StatusCode = 200);

    private static readonly BaitResponse WpLogin = new(
        Name: "wp-login",
        ContentType: "text/html; charset=utf-8",
        Body: """
            <!DOCTYPE html>
            <html><head><title>Log In &lsaquo; WordPress</title></head>
            <body class="login">
            <div id="login">
            <h1><a href="https://wordpress.org/">Powered by WordPress</a></h1>
            <form name="loginform" id="loginform" action="wp-login.php" method="post">
            <p><label for="user_login">Username or Email Address</label>
            <input type="text" name="log" id="user_login" class="input" value="" size="20"></p>
            <p><label for="user_pass">Password</label>
            <input type="password" name="pwd" id="user_pass" class="input" value="" size="20"></p>
            <p class="submit"><input type="submit" name="wp-submit" id="wp-submit" class="button button-primary button-large" value="Log In"></p>
            </form>
            </div>
            </body></html>
            """);

    private static readonly BaitResponse WpSetup = new(
        Name: "wp-setup",
        ContentType: "text/html; charset=utf-8",
        Body: """
            <!DOCTYPE html>
            <html><head><title>WordPress &rsaquo; Setup Configuration File</title></head>
            <body class="wp-core-ui">
            <h1>Welcome</h1>
            <p>Welcome to WordPress. Before getting started, we need some information on the database.</p>
            <form method="post" action="setup-config.php?step=2">
            <p><label for="dbname">Database Name</label><input name="dbname" id="dbname" type="text" value="wordpress"></p>
            <p><label for="uname">Username</label><input name="uname" id="uname" type="text" value="username"></p>
            <p><label for="pwd">Password</label><input name="pwd" id="pwd" type="password"></p>
            <p><label for="dbhost">Database Host</label><input name="dbhost" id="dbhost" type="text" value="localhost"></p>
            <p class="step"><input type="submit" class="button button-large" value="Submit"></p>
            </form>
            </body></html>
            """);

    private static readonly BaitResponse Xmlrpc = new(
        Name: "xmlrpc",
        ContentType: "text/plain; charset=utf-8",
        Body: "XML-RPC server accepts POST requests only.\n");

    private static readonly BaitResponse DotEnv = new(
        Name: "dotenv",
        ContentType: "text/plain; charset=utf-8",
        Body: """
            APP_NAME=Laravel
            APP_ENV=production
            APP_KEY=base64:BwG3xR9pQ7vN2mK5jH8dF4aL0cZ6yT1sV+eU=
            APP_DEBUG=false
            APP_URL=https://mcarthey.com

            LOG_CHANNEL=stack
            LOG_LEVEL=warning

            DB_CONNECTION=mysql
            DB_HOST=127.0.0.1
            DB_PORT=3306
            DB_DATABASE=app_prod
            DB_USERNAME=app_user
            DB_PASSWORD=hunter2

            REDIS_HOST=127.0.0.1
            REDIS_PORT=6379

            MAIL_MAILER=smtp
            MAIL_HOST=smtp.mailgun.org
            MAIL_PORT=587
            MAIL_USERNAME=postmaster@example.com
            MAIL_PASSWORD=changeme
            MAIL_ENCRYPTION=tls
            """);

    private static readonly BaitResponse GitConfig = new(
        Name: "git-config",
        ContentType: "text/plain; charset=utf-8",
        Body: """
            [core]
            	repositoryformatversion = 0
            	filemode = true
            	bare = false
            	logallrefupdates = true
            [remote "origin"]
            	url = git@github.com:example/private-repo.git
            	fetch = +refs/heads/*:refs/remotes/origin/*
            [branch "main"]
            	remote = origin
            	merge = refs/heads/main
            """);

    private static readonly BaitResponse GitHead = new(
        Name: "git-head",
        ContentType: "text/plain; charset=utf-8",
        Body: "ref: refs/heads/main\n");

    private static readonly BaitResponse PhpMyAdmin = new(
        Name: "phpmyadmin",
        ContentType: "text/html; charset=utf-8",
        Body: """
            <!DOCTYPE html>
            <html><head><title>phpMyAdmin</title></head>
            <body>
            <div id="page_content">
            <form method="post" action="index.php" name="login_form" id="login_form" class="disableAjax login hide js-show">
            <fieldset><legend>Log in</legend>
            <div class="item"><label for="input_username">Username:</label><input type="text" name="pma_username" id="input_username" value="" size="24" class="textfield" title="Username" autocomplete="username"></div>
            <div class="item"><label for="input_password">Password:</label><input type="password" name="pma_password" id="input_password" value="" size="24" class="textfield" title="Password" autocomplete="current-password"></div>
            <div class="item"><label for="select_server">Server:</label><select name="server" id="select_server"><option value="1">localhost</option></select></div>
            </fieldset>
            <fieldset class="tblFooters"><input type="submit" name="submit" value="Go" id="input_go"></fieldset>
            </form>
            </div>
            </body></html>
            """);

    private static readonly BaitResponse ConfigPhp = new(
        Name: "config-php",
        ContentType: "text/plain; charset=utf-8",
        Body: """
            <?php
            // Database configuration
            define('DB_HOST', 'localhost');
            define('DB_USER', 'app_user');
            define('DB_PASSWORD', 'p@ssw0rd123');
            define('DB_NAME', 'app_prod');

            define('APP_SECRET', 'app_prod_r4nd0mL00k1ngValueTh4tB0tsW1llTry');
            define('SITE_URL', 'https://mcarthey.com');
            ?>
            """);

    private static readonly BaitResponse ActuatorEnv = new(
        Name: "actuator-env",
        ContentType: "application/json",
        Body: """
            {
              "activeProfiles": ["production"],
              "propertySources": [
                {
                  "name": "systemProperties",
                  "properties": {
                    "user.timezone": {"value": "UTC"},
                    "java.version": {"value": "17.0.8"}
                  }
                },
                {
                  "name": "applicationConfig: [classpath:/application.yml]",
                  "properties": {
                    "spring.datasource.url": {"value": "jdbc:postgresql://localhost:5432/appdb"},
                    "spring.datasource.username": {"value": "appuser"},
                    "spring.datasource.password": {"value": "******"}
                  }
                }
              ]
            }
            """);

    private static readonly BaitResponse ActuatorHealth = new(
        Name: "actuator-health",
        ContentType: "application/json",
        Body: """{"status":"UP","components":{"db":{"status":"UP"},"diskSpace":{"status":"UP"}}}""");

    private static readonly BaitResponse TomcatManager = new(
        Name: "tomcat-manager",
        ContentType: "text/html; charset=utf-8",
        Body: "<html><body><h1>401 Unauthorized</h1><p>You are not authorized to view this page. Please log in with a valid manager account.</p></body></html>",
        StatusCode: 401);

    // Legacy portfolio routes that used to be real pages. Now they log as
    // "restricted" and serve a themed 401 -- bots that scraped the old URLs
    // land in the shame log; humans who bookmarked the old pages get a
    // friendly path back to the console.
    // Meta-cheat bait: URLs a "clever" hacker might POST to directly to
    // try to bypass the puzzle. Every hit is logged as bait "meta-cheat"
    // and shows on the wall of shame under a "well played but noted"
    // category. Response is intentionally boring -- 404-style JSON.
    private static readonly BaitResponse MetaCheat = new(
        Name: "meta-cheat",
        ContentType: "application/json",
        Body: """{"error":"not found","hint":"nice try"}""",
        StatusCode: 404);

    private static readonly BaitResponse Restricted = new(
        Name: "restricted",
        ContentType: "text/html; charset=utf-8",
        Body: """
            <!DOCTYPE html>
            <html lang="en">
            <head>
            <meta charset="UTF-8">
            <title>401 - Access Restricted</title>
            <style>
            body { background: #000; color: #33ff33; font-family: 'Courier New', monospace; padding: 40px; text-align: center; }
            a { color: #ffbf00; text-decoration: none; border-bottom: 1px dotted #ffbf00; }
            .box { border: 1px solid #33ff33; padding: 40px; max-width: 520px; margin: 60px auto; }
            pre { text-align: left; margin: 0; white-space: pre-wrap; line-height: 1.6; }
            </style>
            </head>
            <body>
            <div class="box">
            <pre>&gt; ACCESS DENIED

              HTTP 401 - Authentication required.
              This attempt has been logged.

              Return to <a href="/">system console</a>.</pre>
            </div>
            </body>
            </html>
            """,
        StatusCode: 401);

    private static readonly BaitResponse Adminer = new(
        Name: "adminer",
        ContentType: "text/html; charset=utf-8",
        Body: """
            <!DOCTYPE html>
            <html><head><title>Login - Adminer</title></head>
            <body>
            <form action="" method="post">
            <table>
            <tr><th>System<td><select name='auth[driver]'><option value='server'>MySQL</option><option value='pgsql'>PostgreSQL</option></select>
            <tr><th>Server<td><input name='auth[server]' value='db' size='24'>
            <tr><th>Username<td><input name='auth[username]' value='' size='24'>
            <tr><th>Password<td><input type='password' name='auth[password]' size='24'>
            <tr><th>Database<td><input name='auth[db]' value='' size='24'>
            </table>
            <p><input type='submit' value='Login'>
            </form>
            </body></html>
            """);

    // Path → response mapping. Keys are compared lowercased.
    // Aliases point at the same BaitResponse so log analytics see one bait per family.
    public static readonly FrozenDictionary<string, BaitResponse> Table = new Dictionary<string, BaitResponse>(StringComparer.OrdinalIgnoreCase)
    {
        ["/wp-login.php"] = WpLogin,
        ["/wp-admin/"] = WpLogin,
        ["/wp-admin"] = WpLogin,
        ["/wp-admin/setup-config.php"] = WpSetup,
        ["/wordpress/wp-login.php"] = WpLogin,
        ["/blog/wp-login.php"] = WpLogin,

        ["/xmlrpc.php"] = Xmlrpc,

        ["/.env"] = DotEnv,
        ["/.env.local"] = DotEnv,
        ["/.env.production"] = DotEnv,
        ["/api/.env"] = DotEnv,

        ["/.git/config"] = GitConfig,
        ["/.git/HEAD"] = GitHead,

        ["/phpmyadmin/"] = PhpMyAdmin,
        ["/phpmyadmin"] = PhpMyAdmin,
        ["/phpmyadmin/index.php"] = PhpMyAdmin,
        ["/pma/"] = PhpMyAdmin,
        ["/pma"] = PhpMyAdmin,
        ["/mysql/"] = PhpMyAdmin,
        ["/mysqladmin/"] = PhpMyAdmin,

        ["/config.php"] = ConfigPhp,
        ["/wp-config.php"] = ConfigPhp,

        ["/actuator/env"] = ActuatorEnv,
        ["/actuator/health"] = ActuatorHealth,

        ["/manager/html"] = TomcatManager,
        ["/host-manager/html"] = TomcatManager,

        ["/adminer.php"] = Adminer,
        ["/adminer/"] = Adminer,

        // Meta-cheat bait: someone trying to bypass the puzzle by POSTing
        // to guessed endpoints. Every hit shame-walled as "meta-cheat".
        ["/api/flag"] = MetaCheat,
        ["/api/hack/complete"] = MetaCheat,
        ["/api/hack/grant"] = MetaCheat,
        ["/api/hack/skip"] = MetaCheat,
        ["/api/hack/solve"] = MetaCheat,
        ["/api/session/promote"] = MetaCheat,
        ["/api/admin/login"] = MetaCheat,
        ["/api/admin/grant"] = MetaCheat,
        ["/flag"] = MetaCheat,
        ["/flag.txt"] = MetaCheat,
        ["/.well-known/flag"] = MetaCheat,
        ["/challenge.txt"] = MetaCheat,

        // Legacy portfolio routes -- now 401 traps.
        ["/About"] = Restricted,
        ["/About/"] = Restricted,
        ["/About/Index"] = Restricted,
        ["/Projects"] = Restricted,
        ["/Projects/"] = Restricted,
        ["/Projects/Index"] = Restricted,
        ["/Hobbies"] = Restricted,
        ["/Hobbies/"] = Restricted,
        ["/Hobbies/Index"] = Restricted,
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
