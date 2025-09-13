using HomeFinder.API.Models.Repository;
using HomeFinder.Entity.DB;
using Mapster;
namespace HomeFinder.API.Models.Service
{
    public class ItemService
    {
        private readonly ItemRepository _itemRepository;

        public ItemService(ItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<ItemDTO> AddItemAsync(ItemDTO itemDto)
        {
            var item = itemDto.Adapt<Item>();
            var addedItem = await _itemRepository.AddAsync(item);
            return addedItem.Adapt<ItemDTO>();
        }

        public async Task<ItemDTO> GetItemAsync(int id)
        {
            var item = await _itemRepository.GetByIdAsync(id);
            return item?.Adapt<ItemDTO>();
        }

        public async Task<IEnumerable<ItemDTO>> GetAllItemsAsync()
        {
            var items = await _itemRepository.GetAllAsync();
            return items.Select(x => x.Adapt<ItemDTO>());
        }

        public async Task<ItemDTO> UpdateItemAsync(int id, ItemDTO itemDto)
        {
            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null) return null;
            
            itemDto.Adapt(item); // itemDtoの値をitemにマッピング

            var updatedItem = await _itemRepository.UpdateAsync(item);
            return updatedItem.Adapt<ItemDTO>();
        }
        
        public async Task DeleteItemAsync(int id)
        {
            await _itemRepository.DeleteAsync(id);
        }
    }
}