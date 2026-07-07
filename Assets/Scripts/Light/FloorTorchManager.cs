using UnityEngine;                                                                                            
    using System.Collections;                                                                                     
                                                                                                                  
    public class FloorTorchManager : MonoBehaviour                                                                
    {                                                                                                             
        [Header("Prefab Ánh Sáng để gắn vào nhân vật")]                                                           
        [SerializeField] private GameObject playerTorchPrefab;                                                    
                                                                                                                  
        [Header("Tag của nhân vật cần tìm")]                                                                      
        [SerializeField] private string playerTag = "Player";                                                     
                                                                                                                  
        private void Start()                                                                                      
        {                                                                                                         
            // Chạy tiến trình quét tìm player định kỳ                                                            
            StartCoroutine(CheckAndAttachTorchRoutine());                                                         
        }                                                                                                         
                                                                                                                  
        private IEnumerator CheckAndAttachTorchRoutine()                                                          
        {                                                                                                         
            // Vòng lặp vô hạn chạy suốt thời gian ở trong map này                                                
            while (true)                                                                                          
            {                                                                                                     
                // Tìm tất cả GameObject có tag "Player" trong scene                                              
                GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);                              
                                                                                                                  
                foreach (GameObject player in players)                                                            
                {                                                                                                 
                    // Kiểm tra xem player này đã được gắn TorchFlicker (đuốc) chưa                               
                    TorchFlicker existingTorch = player.GetComponentInChildren<TorchFlicker>();                   
                                                                                                                  
                    if (existingTorch == null && playerTorchPrefab != null)                                       
                    {                                                                                             
                        // Nếu chưa có, tiến hành copy (Instantiate) prefab đuốc                                  
                        // và gán nó làm con (Child) của Player                                                   
                        GameObject newTorch = Instantiate(playerTorchPrefab, player.transform);                   
                                                                                                                  
                        // Chỉnh vị trí đuốc vào giữa thân nhân vật (nâng lên 0.5f tùy theo sprite của bạn)       
                        newTorch.transform.localPosition = new Vector3(0, 0.5f, 0);                               
                                                                                                                  
                        Debug.Log($"[FloorTorchManager] Đã tự động gắn đuốc cho: {player.name}");                 
                    }                                                                                             
                }                                                                                                 
                                                                                                                  
                // Dừng 0.5 giây rồi mới quét lại (Cực kỳ tối ưu, không làm nặng game)                            
                yield return new WaitForSeconds(0.5f);                                                            
            }                                                                                                     
        }                                                                                                         
    }                                