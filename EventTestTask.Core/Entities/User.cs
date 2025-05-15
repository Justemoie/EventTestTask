namespace EventTestTask.Core.Entities;

public class User
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public DateTime BirthDate { get; private set; }

    public IList<Registration> Events { get; private set; } = new List<Registration>();

    public string Email { get; private set; }

    public User(Guid id, string firstName, string lastName, DateTime birthDate, string email)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        Email = email;
    }
}