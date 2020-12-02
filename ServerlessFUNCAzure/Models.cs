using System;
//using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.Cosmos.Table;

namespace ServerlessFUNCAzure
{
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedTime = DateTime.UtcNow;
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TodoCreateModel
    {
        public string TaskDescription { get; set; }
    }

    public class TodoUpdateModel
    {
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TodoTableEntity : TableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }

        void abc()
        {
            Mappings.ToTableEntity(new Todo());
            new Todo().ToTableEntity();
        }
    }


    public static class Mappings
    {
        public static TodoTableEntity ToTableEntity(this Todo todo) => new TodoTableEntity()
        {
            PartitionKey = "TODO",
            RowKey = todo.Id,
            CreatedTime = todo.CreatedTime,
            IsCompleted = todo.IsCompleted,
            TaskDescription = todo.TaskDescription
        };

        public static Todo ToTodo(this TodoTableEntity todo) => new Todo()
        {
            Id = todo.RowKey,
            CreatedTime = todo.CreatedTime,
            IsCompleted = todo.IsCompleted,
            TaskDescription = todo.TaskDescription
        };

    }

}
