// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SeedNotes.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : AINotesApp
// Project Name :  AINotesApp
// =======================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AINotesApp.Data;
using AINotesApp.Services;

using MediatR;

namespace AINotesApp.Features.Notes.SeedNotes;

/// <summary>
///   Command to seed the database with sample notes for testing.
/// </summary>
public record SeedNotesCommand : IRequest<SeedNotesResponse>
{

	/// <summary>
	///   Gets the Auth0 subject to create notes for.
	/// </summary>
	public string UserSubject { get; init; } = string.Empty;

	/// <summary>
	///   Gets the number of notes to create.
	/// </summary>
	public int Count { get; init; } = 50;

}

/// <summary>
///   Handler for seeding sample notes with AI summaries and embeddings.
/// </summary>
public class SeedNotesHandler : IRequestHandler<SeedNotesCommand, SeedNotesResponse>
{

	private readonly IAiService _aiService;

	private readonly ApplicationDbContext _context;

	public SeedNotesHandler(ApplicationDbContext context, IAiService aiService)
	{
		_context = context;
		_aiService = aiService;
	}

	/// <summary>
	///   Handles the command to seed notes.
	/// </summary>
	public async Task<SeedNotesResponse> Handle(SeedNotesCommand request, CancellationToken cancellationToken)
	{
		var sampleNotes = GetSampleNotes();
		var createdNotes = new List<Guid>();
		var errors = new List<string>();

		// Take only the requested count
		var notesToCreate = sampleNotes.Take(request.Count).ToList();

		foreach (var (title, content) in notesToCreate)
		{
			try
			{
				// Generate AI summary, tags, and embedding using the real OpenAI API
				var summaryTask = _aiService.GenerateSummaryAsync(content, cancellationToken);
				var tagsTask = _aiService.GenerateTagsAsync(title, content, cancellationToken);
				var embeddingTask = _aiService.GenerateEmbeddingAsync(content, cancellationToken);

				await Task.WhenAll(summaryTask, tagsTask, embeddingTask);

				var note = new Note
				{
						Id = Guid.NewGuid(),
						Title = title,
						Content = content,
						AiSummary = await summaryTask,
						Tags = await tagsTask,
						Embedding = await embeddingTask,
						OwnerSubject = request.UserSubject,
						CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 30)),
						UpdatedAt = DateTime.UtcNow
				};

				_context.Notes.Add(note);
				createdNotes.Add(note.Id);

				// Save periodically to avoid memory issues
				if (createdNotes.Count % 5 == 0)
				{
					await _context.SaveChangesAsync(cancellationToken);
				}
			}
			catch (Exception ex)
			{
				errors.Add($"Failed to create '{title}': {ex.Message}");
			}
		}

		// Final save
		await _context.SaveChangesAsync(cancellationToken);

		return new SeedNotesResponse
		{
				CreatedCount = createdNotes.Count,
				CreatedNoteIds = createdNotes,
				Errors = errors
		};
	}

	/// <summary>
	///   Gets a list of sample notes across various topics.
	/// </summary>
	private static List<(string Title, string Content)> GetSampleNotes()
	{
		return new List<(string, string)>
		{
				// Video Games (10 notes)
				("The Legend of Zelda: Breath of the Wild",
						"An open-world action-adventure game that revolutionized the Zelda franchise. Players explore the vast kingdom of Hyrule, solving puzzles, fighting enemies, and uncovering the story of Link's battle against Calamity Ganon. The game features a physics-based engine, dynamic weather, and complete freedom in how you approach objectives."),

				("Dark Souls Game Design",
						"Dark Souls is renowned for its challenging combat and interconnected world design. The game teaches players through failure, with each death being a learning opportunity. The stamina-based combat system requires careful timing and resource management. Level design features shortcuts that loop back to earlier areas, creating a sense of place."),

				("Minecraft Building Techniques",
						"Minecraft allows unlimited creativity through its block-based building system. Advanced techniques include redstone circuits for automation, terraforming landscapes, and creating pixel art. Players can build anything from medieval castles to modern skyscrapers. The game encourages experimentation and problem-solving."),

				("Elden Ring Lore and Story",
						"Elden Ring combines FromSoftware's signature gameplay with an open-world structure. The lore, written by George R.R. Martin, tells the story of the Lands Between and the shattering of the Elden Ring. Players piece together the narrative through item descriptions, NPC dialogue, and environmental storytelling."),

				("Stardew Valley Farming Guide",
						"Stardew Valley is a farming simulation RPG where you inherit your grandfather's farm. Optimize your farm layout for maximum profit, build relationships with townspeople, explore mines for resources, and participate in seasonal festivals. The game emphasizes relaxation and player choice."),

				("Portal Puzzle Mechanics",
						"Portal introduces innovative first-person puzzle gameplay using a portal gun. Players create interconnected portals to solve spatial reasoning puzzles. The game teaches complex concepts gradually, introducing momentum-based puzzles and aerial navigation. GLaDOS provides darkly humorous commentary throughout."),

				("Red Dead Redemption 2 Immersion",
						"Red Dead Redemption 2 features incredible attention to detail in its Wild West setting. Arthur Morgan's journey explores themes of loyalty, morality, and the end of the outlaw era. The game's realistic systems include horse bonding, weapon maintenance, and dynamic NPC interactions."),

				("Hollow Knight Metroidvania Design",
						"Hollow Knight exemplifies modern Metroidvania design with its interconnected underground kingdom. Players gradually unlock abilities that open new areas. The game features tight combat, atmospheric music, and challenging boss fights. The hand-drawn art style creates a hauntingly beautiful world."),

				("The Witcher 3 Quest Design",
						"The Witcher 3 sets new standards for RPG quests with morally complex choices and consequences. Geralt's contracts involve investigating monsters, making difficult decisions, and dealing with political intrigue. Even minor side quests feature compelling stories and memorable characters."),

				("Hades Roguelike Progression",
						"Hades combines roguelike gameplay with a persistent narrative. Each escape attempt from the underworld provides permanent upgrades and story progression. The game features tight combat, diverse weapon builds, and exceptional voice acting. Death becomes part of the narrative rather than pure punishment."),

				// Movies (10 notes)
				("Inception Dream Layers Explained",
						"Christopher Nolan's Inception explores dreams within dreams as a heist team performs extraction and inception. The film's layered structure includes reality, the first dream, second dream, third dream, and limbo. Time dilation increases with each level. The spinning top totem raises questions about the ending's reality."),

				("The Shawshank Redemption Themes",
						"The Shawshank Redemption explores hope, friendship, and redemption. Andy Dufresne maintains hope during wrongful imprisonment while forming a deep friendship with Red. The film examines institutionalization and the power of human spirit. The climactic escape and reunion celebrate persistence and faith."),

				("Blade Runner 2049 Cinematography",
						"Blade Runner 2049 features stunning cinematography by Roger Deakins. The film uses color, lighting, and composition to create a dystopian future. Orange and blue dominate the palette. Each frame resembles a painting. The visual storytelling enhances themes of memory, identity, and what makes us human."),

				("Parasite Social Commentary",
						"Bong Joon-ho's Parasite examines class inequality through the Kim family's infiltration of the wealthy Park household. The film uses vertical space to represent social hierarchy. Genre shifts keep viewers off-balance. The semi-basement symbolizes the family's social position. Dark comedy masks sharp social critique."),

				("The Matrix Philosophy and Themes",
						"The Matrix blends action with philosophical questions about reality, free will, and control. The film references Plato's Cave, Descartes' skepticism, and Buddhist concepts of illusion. The choice between red and blue pills represents awakening versus comfortable ignorance. Humanity's relationship with technology serves as central metaphor."),

				("Pulp Fiction Narrative Structure",
						"Tarantino's Pulp Fiction employs non-linear storytelling with interconnected vignettes. The circular structure begins and ends at the diner. Characters reappear in different contexts. The briefcase serves as a MacGuffin. Sharp dialogue and pop culture references define the style. Violence and dark humor blend seamlessly."),

				("Interstellar Science and Emotion",
						"Interstellar balances hard science fiction with emotional father-daughter story. The film explores relativity, black holes, and time dilation based on real physics. Cooper's journey through space parallels Murphy's life on Earth. Love becomes a quantifiable dimension. Hans Zimmer's score amplifies emotional moments."),

				("Mad Max Fury Road Visual Storytelling",
						"George Miller's Fury Road tells its story primarily through action and visuals. Minimal dialogue lets the chase speak. Practical effects create visceral impact. The film subverts expectations with Furiosa as the true protagonist. Color grading shifts from orange wasteland to blue-green hope."),

				("Arrival Linguistics and Time",
						"Arrival explores communication through alien linguistics. Learning the heptapod language grants Louise non-linear time perception. The Sapir-Whorf hypothesis becomes literal. The film examines grief, choice, and determinism. Amy Adams delivers a powerful performance about choosing to live fully despite knowing future pain."),

				("The Grand Budapest Hotel Symmetry",
						"Wes Anderson's Grand Budapest Hotel showcases his signature symmetrical compositions and pastel color palettes. The film uses different aspect ratios for different time periods. Miniatures and practical effects create a handcrafted feel. The caper story contains deeper themes of civilization, war, and nostalgia."),

				// Programming (15 notes)
				("JavaScript Async/Await Patterns",
						"Async/await simplifies asynchronous JavaScript code. The async keyword makes functions return promises. Await pauses execution until promises resolve. Error handling uses try/catch blocks. Common patterns include parallel execution with Promise.all, sequential operations, and error propagation. Avoid mixing callbacks with async/await."),

				("React Hooks Best Practices",
						"React Hooks enable functional components to use state and lifecycle features. useState manages component state. useEffect handles side effects. Custom hooks extract reusable logic. Follow the Rules of Hooks: only call at top level, only call from React functions. useMemo and useCallback optimize performance."),

				("Database Indexing Strategies",
						"Database indexes speed up queries but slow down writes. B-tree indexes work well for range queries. Hash indexes optimize exact matches. Composite indexes support multiple columns. Consider query patterns when designing indexes. Avoid over-indexing. Monitor index usage and remove unused ones. Covering indexes include all queried columns."),

				("RESTful API Design Principles",
						"RESTful APIs use HTTP methods semantically: GET for retrieval, POST for creation, PUT for updates, DELETE for removal. URLs represent resources, not actions. Use nouns, not verbs. Return appropriate status codes. Support pagination for large datasets. Version your API. Implement proper error handling with meaningful messages."),

				("Git Branching Strategies",
						"Git Flow uses master, develop, feature, release, and hotfix branches. GitHub Flow simplifies to main branch and feature branches. Trunk-based development keeps branches short-lived. Feature flags enable deploying incomplete features. Rebase vs merge affects history cleanliness. Squash commits for clean history."),

				("Clean Code Principles",
						"Clean code emphasizes readability and maintainability. Use meaningful variable and function names. Functions should do one thing. Keep functions small. Avoid comments by writing self-documenting code. Follow consistent formatting. Minimize dependencies. Apply SOLID principles. Refactor regularly to prevent code rot."),

				("Microservices Architecture Patterns",
						"Microservices decompose applications into small, independent services. Each service owns its database. Communicate via APIs or message queues. Use API gateways for client requests. Implement circuit breakers for resilience. Service discovery enables dynamic scaling. Distributed tracing monitors cross-service requests."),

				("TypeScript Generics Tutorial",
						"TypeScript generics create reusable, type-safe components. Generic functions work with multiple types while maintaining type information. Constraints limit generic types using extends. Generic classes and interfaces enable flexible data structures. Utility types like Partial, Pick, and Omit simplify type manipulation."),

				("Docker Container Optimization",
						"Optimize Docker images by using multi-stage builds to reduce size. Choose minimal base images like Alpine. Combine RUN commands to minimize layers. Order Dockerfile commands from least to most frequently changing. Use .dockerignore to exclude unnecessary files. Leverage build cache effectively."),

				("OAuth 2.0 Authentication Flow",
						"OAuth 2.0 provides secure delegated access. Authorization code flow works for server-side apps. Implicit flow suits single-page applications. Client credentials flow serves machine-to-machine authentication. Refresh tokens extend access without re-authentication. Scopes limit permissions granted. Always use HTTPS."),

				("GraphQL vs REST Comparison",
						"GraphQL allows clients to request exactly needed data, avoiding over-fetching and under-fetching. Single endpoint handles all requests. Strong typing enables better tooling. REST uses multiple endpoints with fixed response structures. GraphQL complexity comes from resolver implementation. REST caching is simpler."),

				("Python List Comprehensions",
						"List comprehensions provide concise syntax for creating lists. Basic syntax: [expression for item in iterable]. Add conditions with if clauses. Nested comprehensions handle multidimensional data. Dictionary and set comprehensions work similarly. Comprehensions are often faster than explicit loops due to optimization."),

				("SQL Query Optimization",
						"Optimize SQL by selecting only needed columns. Use WHERE to filter early. Joins can be expensive; consider alternatives. Analyze query execution plans. Avoid SELECT * in production. Use appropriate indexes. Batch operations when possible. Denormalization sometimes improves read performance."),

				("Kubernetes Deployment Basics",
						"Kubernetes orchestrates containerized applications. Pods are the smallest deployable units. Deployments manage pod replicas. Services provide stable networking. ConfigMaps and Secrets manage configuration. Namespaces organize resources. Ingress controllers route external traffic. Use health checks for reliability."),

				("Test-Driven Development Workflow",
						"TDD follows red-green-refactor cycle. Write failing test first (red). Implement minimum code to pass (green). Refactor while maintaining tests. Benefits include better design, living documentation, and confidence in changes. Mock external dependencies. Keep tests fast and isolated. Aim for high coverage."),

				// Fantasy Books (15 notes)
				("The Lord of the Rings World-Building",
						"Tolkien's Middle-earth features deep world-building with invented languages, histories, and cultures. The geography includes the Shire, Mordor, Gondor, and Rohan. Different races have distinct characteristics and histories. The lore extends thousands of years before the main story. Attention to detail creates immersive authenticity."),

				("Brandon Sanderson Magic Systems",
						"Sanderson's magic systems follow consistent rules. Allomancy in Mistborn burns metals for specific powers. Surgebinding in Stormlight Archive bonds spren for abilities. Hard magic systems enable clever problem-solving. Limitations make magic interesting. Costs prevent magic from solving everything. Rules create strategic depth."),

				("Game of Thrones Political Intrigue",
						"A Song of Ice and Fire features complex political maneuvering among noble houses. Characters scheme for the Iron Throne using alliances, betrayals, and marriages. Martin subverts fantasy tropes by killing major characters. Multiple POV chapters show different perspectives. Gray morality makes all sides sympathetic."),

				("The Wheel of Time Prophecy",
						"Robert Jordan's series revolves around prophecies and the Dragon Reborn. The Pattern weaves people's lives toward destiny. Multiple magic systems include channeling, dreamwalking, and ter'angreal. Epic scope spans fourteen books. Characters grow from village youth to world leaders. Balance between determinism and choice."),

				("Harry Potter Coming of Age",
						"Harry Potter combines magical school life with hero's journey. Each book corresponds to a school year and Harry's maturation. The series grows darker as Harry ages. Themes include death, choice, and prejudice. Found family provides emotional core. The magical world hides within mundane Britain."),

				("The Name of the Wind Storytelling",
						"Patrick Rothfuss uses frame narrative with Kvothe telling his own story. The unreliable narrator adds layers of interpretation. Beautiful prose emphasizes music and words. Sympathy magic system connects objects through similarity. The University provides magical education setting. Mystery surrounds Kvothe's fall from legend."),

				("Malazan Character Complexity",
						"Malazan Book of the Fallen features morally complex characters without clear heroes or villains. Gods walk among mortals. Convergence brings storylines together. Multiple continents and thousands of years of history. Dense philosophical themes about power, compassion, and civilization. Requires patient reading but rewards investment."),

				("The First Law Grimdark Fantasy",
						"Joe Abercrombie's First Law trilogy exemplifies grimdark fantasy. Characters are selfish, violent, and flawed. War brings suffering, not glory. Torture and moral compromise feature prominently. Dark humor balances brutality. Characters change little despite experiences. Cynical worldview subverts traditional fantasy."),

				("Discworld Satire and Humor",
						"Terry Pratchett's Discworld satirizes our world through fantasy lens. The flat world rests on elephants standing on a giant turtle. Each book explores themes like justice, religion, or media. Witches, wizards, and Death appear throughout. Clever wordplay and footnotes add humor. Social commentary underneath comedy."),

				("The Broken Earth Apocalypse",
						"N.K. Jemisin's trilogy features a tectonically unstable world called the Stillness. Orogenes control earth and heat but face persecution. Each book parallels a season�apocalyptic event. Second-person narration creates intimacy. Themes of oppression, environmentalism, and power. Won three consecutive Hugo Awards."),

				("The Lies of Locke Lamora Heist",
						"Scott Lynch crafts elaborate cons in fantasy Venice. The Gentlemen Bastards pull off impossible thefts. Flashback structure reveals character backgrounds. Camorr features detailed world-building with Eldren ruins and garristas. Friendship between Locke and Jean drives story. Dark humor and violence coexist."),

				("The Kingkiller Chronicle Magic",
						"Sympathy creates links between objects to transfer force. Naming speaks true names of things for direct control. The University teaches systematic magic while Naming remains mysterious. Kvothe's genius and arrogance shape his education. Music provides magical and emotional depth. Waiting for book three continues."),

				("Red Rising Class Warfare",
						"Pierce Brown's series uses color-coded castes in future society. Golds rule while Reds mine. Darrow infiltrates Gold society to bring revolution. Gladiatorial school tests students. Brutality serves liberation goals. Fast-paced action combines with political strategy. Science fiction with fantasy feel."),

				("The Dresden Files Urban Fantasy",
						"Jim Butcher blends detective noir with wizardry in modern Chicago. Harry Dresden works as wizard private investigator. Fae courts, vampires, and demons operate in shadows. Magic follows rules and has costs. Series grows in scope from local problems to cosmic threats. Humor balances dark elements."),

				("The Poppy War Historical Fantasy",
						"R.F. Kuang bases fantasy on Second Sino-Japanese War. Rin discovers shamanic powers at military academy. Drugs open connection to gods. War crimes and genocide feature prominently. Explores costs of power and vengeance. Academic setting gives way to brutal warfare. Historical parallel adds weight.")
		};
	}

}

/// <summary>
///   Response after seeding notes.
/// </summary>
public record SeedNotesResponse
{

	/// <summary>
	///   Gets the number of notes successfully created.
	/// </summary>
	public int CreatedCount { get; init; }

	/// <summary>
	///   Gets the IDs of created notes.
	/// </summary>
	public List<Guid> CreatedNoteIds { get; init; } = new();

	/// <summary>
	///   Gets any errors that occurred during seeding.
	/// </summary>
	public List<string> Errors { get; init; } = new();

}